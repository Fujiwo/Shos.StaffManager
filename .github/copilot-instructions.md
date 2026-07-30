# Shos.StaffManager — Copilot instructions

Japanese-language, console-based staff/department management sample built with .NET 10.0 / C#. The solution (`Shos.StaffManager.sln`) has three projects: a console app with an embedded AI chat, a shared models library, and a standalone MCP server that exposes the same data to external AI assistants (Claude Desktop, VS Code, etc.).

## Commands

Run from the repo root.

```bash
dotnet build                                    # build everything
dotnet build Shos.StaffManager.MCPServer        # build a single project
dotnet run --project Shos.StaffManager          # run the console app
dotnet run --project Shos.StaffManager.MCPServer# run the MCP server directly

# inspect the MCP server interactively
npx @modelcontextprotocol/inspector dotnet run --project ./Shos.StaffManager.MCPServer/Shos.StaffManager.MCPServer.csproj
```

There is **no test project** and no lint config beyond the built-in .NET analyzers (`EnableNETAnalyzers` + `AnalysisLevel=latest-Minimum`, set in every `.csproj`); `dotnet build` is what surfaces warnings. Do not add a test project or CI unless explicitly asked.

The MCP server is registered for local VS Code use in `.vscode/mcp.json` (server id `my-mcp-server-95787fc8`, stdio transport, absolute project path).

## Solution structure

| Project | Purpose |
| --- | --- |
| `Shos.StaffManager` | Console app (`OutputType=Exe`). Entry point `Shos.StaffManager/Program.cs`. Packages: `Shos.Console`, `Microsoft.Agents.AI(.OpenAI)`, `Azure.AI.OpenAI`, `ModelContextProtocol`, `OllamaSharp`. |
| `Shos.StaffManager.Models` | Shared domain/persistence library (`Models.cs`), referenced by both other projects. Package: `Shos.UndoRedoList`. |
| `Shos.StaffManager.MCPServer` | Standalone MCP server executable (`Program.cs`). Packages: `Microsoft.Extensions.Hosting`, `ModelContextProtocol`. |

`Documents/ClassDiagrams.md` holds Mermaid namespace/class diagrams for the whole solution — check there first instead of re-deriving the type graph from source, and update it when you add or rename public types.

## Architecture — console app

Flow in `Program.cs`: `Company.Load` → `ShowTitle` → loop `CommandManager.Run(company)` until it returns `false` → `Company.Save`. The data file path is hardcoded (`const string dataFilePath = @"C:\work\FC.StaffManager.json"`) — **not** the sample under `Shos.StaffManager/Data/`.

- **`Shos.StaffManager.Models` (`Models.cs`)** — domain types and persistence.
  - `Department` / `Staff` are `record`s carrying validation *constants* (`MinimumCode`/`MaximumCode`, `MinimumNumber`/`MaximumNumber`, `MinimumNameLength`/`MaximumNameLength`) rather than runtime validation; callers (UI / tool layers) enforce them.
  - `Staff.Department` is a live object reference. `SerializableStaff` is the JSON DTO (stores `DepartmentCode`) with `SerializableStaff.From` / `.To(departmentList)`; `.To` throws `SerializeException` for an unknown code.
  - `DepartmentList` / `StaffList` wrap `Shos.Collections.UndoRedoList<T>` instead of `List<T>`, so `Undo()`/`Redo()` exist on both collections even though no UI code calls them yet.
  - `Company` is the aggregate root: `[JsonIgnore]` live lists plus `[JsonInclude]` private serializable projections (`SerializableDepartmentList`, `SerializableStaffList`) that `System.Text.Json` uses. `Save`/`Load` read/write UTF-8 JSON directly — no repository abstraction, no DB.
  - Domain invariants live here, not in the UI: e.g. `Company.RemoveDepartment` refuses to remove a department that still has staff assigned.
- **`Application/Controllers.cs`** — one `Command<Company>` subclass per menu item, registered by mnemonic in `CommandManager`'s constructor: `s` 社員一覧, `f` 社員検索, `a` 社員追加, `d` 部署一覧, `e` 部署追加, `c` AIチャット, `x` 終了. Multi-step commands (`AddStaffCommand`, `AddDepartmentCommand`, `SearchStaffsCommand`) expose `Steps` as `Func<Company, bool>[]`; a step returning `false` steps **backward** (see `DialogBox<TModel>.Show`), which is how "cancel" inside a wizard returns to the previous prompt instead of aborting. `Command.Mode` (`Once`/`Repeat`/`Exit`) controls whether `CommandManager.Run` loops the command behind a "メイン メニューに戻りますか?" confirmation. `CommandManager.Run` also sets `Toolbox.Company = model` on every iteration, wiring the current in-memory `Company` into the AI tool layer.
- **`Application/View.cs`** — pure display/formatting only; renders `Department`/`Staff` collections through the `Shos.Console` package's `ShowTable()` extension over anonymous projections (`D3` codes, `D4` staff numbers). No business logic here.
- **`Common/ControllerBase.cs`** — reusable console UI primitives: `Window`, `ConfirmBox`, `DialogBox<TModel>` (the step sequencer), `Command<TModel>` / `SingleStepCommand<TModel>`, and `Menu<TModel>` (mnemonic-driven selector).
- **`Common/Helpers.cs`** — cross-cutting utilities: `StringExtensions.Width()`/`IsZenkaku()` compute display width for mixed zenkaku (2) / hankaku (1) text for console alignment; `TypeParser` reflectively calls a type's static `Parse(string)` (falling back to `Convert.ChangeType`); `UserInterface.Get<T>` is the single generic input loop that prompts, parses and re-prompts against `(rule, errorMessage)` validators until valid input or the `CancelString` (`"/"`) is entered — **all** field validation funnels through it.
- **`AI/AIAgent.cs`** — the in-app chat, on the Microsoft Agent Framework (`Microsoft.Agents.AI`). `ChatAgent` is the abstract base handling session lifecycle (lazily creates `AIAgent`/`AgentSession` on first prompt, sends `SystemPrompt` once, then relays each prompt via `Agent.RunAsync`). `MyChatAgent` ("社員名簿管理者") picks its backend at **compile time**: Ollama (`http://localhost:11434`, model `minimax-m3:cloud`) unless the `AZURE_OPENAI` symbol is defined, in which case `Azure.AI.OpenAI` reads `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_API_KEY` from environment variables. `GetToolsAsync` supplies both the local `Toolbox` functions (`AIFunctionFactory.Create`) and an external Brave Search MCP tool spawned via `npx @modelcontextprotocol/server-brave-search` (needs `BRAVE_API_KEY`) — so the console app is an MCP *client* as well.
- **`AI/ToolBox.cs`** — static function-calling surface for the in-app agent: `GetAllDepartments`, `SearchDepartments`, `AddNewDepartment`, `RemoveDepartmentByCode`, `ChangeDepartmentNameByCode`, the `Staff` equivalents, plus `ShowAllDepartments`/`ShowAllStaffs`. It operates on the `Company` injected by `CommandManager.Run`; mutating methods swallow exceptions and return `bool` instead of throwing.

## Architecture — MCP server

`Shos.StaffManager.MCPServer/Program.cs` is a minimal `Microsoft.Extensions.Hosting` app: `Host.CreateEmptyApplicationBuilder` → `AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`. `StaffManagerTools` is a `[McpServerToolType]` static class with its own `static Company company = Company.Load(dataFilePath)` — it **does not share process or state with the console app**; both point at the same hardcoded `C:\work\FC.StaffManager.json` and interact only through that file. Every mutating tool (`AddNewDepartment`, `RemoveDepartmentByCode`, `ChangeDepartmentNameByCode`, `AddNewStaff`, `RemoveStaffbyNumber`, `ChangeStaffsDepartmentByNumber`) calls `Save()` immediately, unlike the console app which saves only on exit — concurrent use is last-writer-wins on `Save`/`Load` boundaries. Keep existing tool names (including the typo-ish `RemoveStaffbyNumber`) stable; renaming breaks configured MCP clients.

## Conventions

- Add a console menu command: new `Command<Company>` subclass in `Application/Controllers.cs`, registered with a mnemonic in `CommandManager`'s constructor. Keep display logic in `View.cs` and validation rules in the `UserInterface.Get<T>` call.
- Add a domain operation: implement it once in `Shos.StaffManager.Models`, then expose it **symmetrically** in both `AI/ToolBox.cs` and `MCPServer`'s `StaffManagerTools` if it should be AI-accessible. The two tool sets are hand-kept in sync — they are not shared code.
- Reuse the `const` limits on `Staff`/`Department` (`MinimumNumber`, `MaximumCode`, `MaximumNameLength`, …) instead of hardcoding ranges at call sites.
- Style follows the existing source: file-scoped `namespace X;`, `using` directives after the namespace in the console app files, XML doc comments (`/// <summary>`) on types and members, expression-bodied members where possible, K&R braces (`if (...) {` / `} catch {`), no braces for single-statement `if`, vertically aligned `=` and field declarations, `Nullable`/`ImplicitUsings` enabled.
- User-facing strings are Japanese; identifiers and doc comments are English (the AI files also use Japanese inline comments). Keep that split.
- Persistence is plain `System.Text.Json` over UTF-8 files — do not introduce EF Core, a DI-based repository, or async I/O unless asked.
- `README.md` and `Documents/ClassDiagrams.md` are user-facing docs; update them when behavior or the type graph changes. Do not create new markdown files to describe your changes.
