# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Shos.StaffManager is a Japanese-language, console-based staff/department management system built with .NET 10.0 / C#. The solution has three projects: a console app with an embedded AI chat, a shared models library, and a standalone MCP server that exposes the same data to external AI assistants (Claude Desktop, VS Code, etc.).

## Commands

Build and run from the repo root (`Shos.StaffManager.sln`).

```bash
# Build everything
dotnet build

# Build a single project
dotnet build Shos.StaffManager.MCPServer

# Run the console app
dotnet run --project Shos.StaffManager

# Run the MCP server directly
dotnet run --project Shos.StaffManager.MCPServer

# Test the MCP server interactively via MCP Inspector
npx @modelcontextprotocol/inspector dotnet run --project ./Shos.StaffManager.MCPServer/Shos.StaffManager.MCPServer.csproj
```

There is no test project in the solution and no lint config beyond the built-in .NET analyzers (`EnableNETAnalyzers` + `AnalysisLevel=latest-Minimum`, enabled in every `.csproj`); `dotnet build` surfaces analyzer warnings.

The MCP server is also registered for local VS Code use in `.vscode/mcp.json` (server id `my-mcp-server-95787fc8`).

## Solution structure

- **`Shos.StaffManager`** — the console app (`OutputType=Exe`). Entry point: `Shos.StaffManager/Program.cs`.
- **`Shos.StaffManager.Models`** — shared data/domain library referenced by both the console app and the MCP server (`Models.cs`).
- **`Shos.StaffManager.MCPServer`** — a separate MCP server executable (`Program.cs`) that re-exposes the same domain operations as MCP tools, with its own in-process `Company` instance and its own JSON file persistence.

Mermaid namespace/class diagrams for the whole solution are kept up to date in `Documents/ClassDiagrams.md` — check there first when you need the full type graph instead of re-deriving it from source.

## Architecture (console app)

Program flow (`Program.cs`): `Company.Load` → title → loop `CommandManager.Run(company)` until it returns false → `Company.Save`. The data file path is currently hardcoded (`const string dataFilePath = @"C:\work\FC.StaffManager.json"`) — not the sample file under `Data/`.

The app is organized as a thin MVC-ish layering with a command pattern for menu operations:

- **`Shos.StaffManager.Models`** (`Models.cs`) — domain types and persistence.
  - `Department` / `Staff` are `record`s with validation-relevant constants (`MinimumCode`/`MaximumCode`, `MinimumNumber`/`MaximumNumber`, `MinimumNameLength`/`MaximumNameLength`) rather than runtime validation — callers (UI/tool layers) are expected to enforce these.
  - `Staff.Department` holds a live object reference; `SerializableStaff` is the DTO used for JSON round-tripping (stores `DepartmentCode` instead of the object) via `SerializableStaff.From`/`.To(departmentList)`.
  - `DepartmentList` / `StaffList` wrap a `Shos.UndoRedoList.UndoRedoList<T>` (NuGet package `Shos.UndoRedoList`) rather than a plain `List<T>`, so `Undo()`/`Redo()` are available on both collections even though nothing in the UI currently calls them.
  - `Company` is the aggregate root: owns both lists, exposes `[JsonIgnore]` live lists plus `[JsonInclude]` private serializable projections (`SerializableDepartmentList`, `SerializableStaffList`) that `System.Text.Json` uses for save/load. `Save`/`Load` read/write UTF-8 JSON directly (no repository abstraction/DB).
  - Domain invariants live here, not in the UI: e.g. `Company.RemoveDepartment` refuses to remove a department that still has staff assigned to it.

- **`Application/Controllers.cs`** (`Shos.StaffManager.Application.Controllers`) — one `Command<Company>` subclass per menu item (`ShowStaffsCommand`, `SearchStaffsCommand`, `AddStaffCommand`, `ShowDepartmentsCommand`, `AddDepartmentCommand`, `AIChatCommand`, `ExitCommand`), registered by mnemonic character in `CommandManager` (`s`/`f`/`a`/`d`/`e`/`c`/`x`). Multi-step commands (e.g. `AddStaffCommand`) implement `Steps` as an array of `Func<Company, bool>` — each step returning `false` steps *backward* (see `DialogBox<TModel>.Show` in `Common/ControllerBase.cs`), which is how "cancel" during a wizard returns to the previous prompt instead of aborting outright. `Command.Mode` (`Once`/`Repeat`/`Exit`) controls whether `CommandManager.Run` loops the command with a "return to main menu?" confirmation.
  - `CommandManager.Run` also sets `Toolbox.Company = model` on every iteration, wiring the current in-memory `Company` into the AI tool layer before each command executes.

- **`Application/View.cs`** (`Shos.StaffManager.Application.Views`) — pure display/formatting; renders `Department`/`Staff` collections as tables via the `Shos.Console` NuGet package's `ShowTable()` extension. No business logic.

- **`Common/ControllerBase.cs`** (`Shos.StaffManager.Common.ControllerBase`) — reusable console UI primitives: `Window` (title bar/separator), `ConfirmBox` (yes/no dialog), `DialogBox<TModel>` (step-sequencer described above), `Command<TModel>`/`SingleStepCommand<TModel>` base classes, and `Menu<TModel>` (mnemonic-driven command selector).

- **`Common/Helpers.cs`** (`Shos.StaffManager.Common.Helpers`) — cross-cutting utilities:
  - `StringExtensions.Width()`/`IsZenkaku()` compute display width for mixed Japanese (zenkaku, width 2) / ASCII (hankaku, width 1) text, used for aligning console output.
  - `TypeParser` reflectively calls a type's static `Parse(string)` (falling back to `Convert.ChangeType`) so `UserInterface.Get<T>` can prompt for arbitrary parseable types generically.
  - `UserInterface.Get<T>` is the generic input loop: prompts, parses, and re-prompts against a list of `(rule, errorMessage)` validators until valid input or `CancelString` ("/") is entered — this is the single place all field validation funnels through.

- **`AI/AIAgent.cs`** (`Shos.StaffManager.AI`) — the in-app AI chat, built on the Microsoft Agent Framework (`Microsoft.Agents.AI`).
  - `ChatAgent` is an abstract base handling session lifecycle: lazily creates the `AIAgent`/`AgentSession` on first prompt, sends `SystemPrompt` once, then relays each user prompt through `Agent.RunAsync`.
  - `MyChatAgent` is the concrete agent ("社員名簿管理者"). Its chat backend is selected at compile time: **Ollama** (`OllamaSharp`, `http://localhost:11434`, model `minimax-m3:cloud` by default) unless the `AZURE_OPENAI` conditional-compilation symbol is defined, in which case it uses `Azure.AI.OpenAI` and reads `AZURE_OPENAI_ENDPOINT`/`AZURE_OPENAI_API_KEY` from environment variables.
  - `MyChatAgent.GetToolsAsync` gives the agent two tool sources: the local `Toolbox` functions (via `AIFunctionFactory.Create`) and an external Brave Search MCP tool (spawned via `npx @modelcontextprotocol/server-brave-search`, requiring a `BRAVE_API_KEY` environment variable) — so the in-app chat is itself an MCP *client* in addition to the separate MCP *server* project.

- **`AI/ToolBox.cs`** — static function-calling surface for the in-app agent. Mirrors the MCP server's tool set (`GetAllDepartments`, `SearchDepartments`, `AddNewDepartment`, `RemoveDepartmentByCode`, `ChangeDepartmentNameByCode`, and the `Staff` equivalents) plus `ShowAllDepartments`/`ShowAllStaffs`, operating on the `Company` instance injected by `CommandManager.Run`. All mutating methods swallow exceptions and return `bool` success/failure rather than throwing.

## Architecture (MCP server)

`Shos.StaffManager.MCPServer/Program.cs` is a minimal `Microsoft.Extensions.Hosting` app (`Host.CreateEmptyApplicationBuilder` → `AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`), separate from the console app's process. `StaffManagerTools` is a `[McpServerToolType]` static class with its own `static Company company = Company.Load(dataFilePath)` — **it does not share process/state with the console app**; both point at the same hardcoded file path (`C:\work\FC.StaffManager.json`) but only interact through that file. Every mutating tool (`AddNewDepartment`, `RemoveDepartmentByCode`, `ChangeDepartmentNameByCode`, `AddNewStaff`, `RemoveStaffbyNumber`, `ChangeStaffsDepartmentByNumber`) calls `Save()` immediately after mutating, unlike the console app which only saves on exit — so if both are run against the same file concurrently, last-writer-wins on `Save`/`Load` boundaries.

## Working in this codebase

- When adding a new menu command in the console app, add a `Command<Company>` subclass in `Application/Controllers.cs` and register it with a mnemonic in `CommandManager`'s constructor; keep display logic in `View.cs` and validation rules passed into `UserInterface.Get<T>`.
- When adding a new domain operation on `Company`/`DepartmentList`/`StaffList`, add it once in `Shos.StaffManager.Models`, then expose it symmetrically in both `AI/ToolBox.cs` (in-app agent) and `Shos.StaffManager.MCPServer/Program.cs` (`StaffManagerTools`) if it should be AI-accessible — the two tool sets are hand-kept in sync, not shared code.
- Validation limits (number/code ranges, name lengths) are defined as `const` fields on `Staff`/`Department` in `Models.cs` — reuse those constants rather than hardcoding limits elsewhere.
