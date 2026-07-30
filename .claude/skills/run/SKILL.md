---
name: run
description: Build and launch Shos.StaffManager (interactive console app) or the MCP server to verify a change actually works, including the hardcoded data-file path and Ollama/MCP prerequisites.
---

# Running Shos.StaffManager

This solution has two independently runnable executables plus a shared models
library (see `CLAUDE.md` for the full architecture). Neither has automated
tests, so "did my change work" has to be verified by actually launching one
of them.

- `Shos.StaffManager` — interactive **console** app (reads `Console.ReadLine()` in a loop; never exits on its own).
- `Shos.StaffManager.MCPServer` — stdio **MCP server** (no console UI; driven by an MCP client/inspector).
- `Shos.StaffManager.Models` — library only, not runnable.

## 1. Build first

```bash
dotnet build
```

Fix any analyzer warnings/errors here before trying to run — both executable
projects have `EnableNETAnalyzers=true`.

## 2. The hardcoded data file (read this before testing list/search flows)

Both `Shos.StaffManager/Program.cs` and `Shos.StaffManager.MCPServer/Program.cs`
hardcode the data path as `C:\work\FC.StaffManager.json` — **not**
`Shos.StaffManager/Data/FC.StaffManager.json` (that's just sample data shipped
in the repo). `Company.Load` silently returns an empty `Company` if the file
doesn't exist, so:

- If you want to test against real sample data (existing departments/staff for
  search, department-code validation, etc.), copy the sample file into place
  first:
  ```powershell
  New-Item -ItemType Directory -Force C:\work | Out-Null
  Copy-Item Shos.StaffManager\Data\FC.StaffManager.json C:\work\FC.StaffManager.json -Force
  ```
- If you just want to test an empty-state flow (e.g. add-department), you can
  skip this — a missing file just means "starts empty."
- The console app only calls `company.Save()` on clean exit (mnemonic `x`).
  The MCP server saves after every mutating tool call. Killing the console app
  mid-session (Ctrl+C) discards unsaved changes — always exit via `x` when you
  need the result persisted.

## 3. Running the console app

It's a menu loop keyed by single-character mnemonics, prompted with a trailing
`:` (see `UserInterface.ShowPrompt`). The main menu:

| Key | Command | Notes |
|-----|---------|-------|
| `s` | 社員一覧 (list staff) | single step |
| `f` | 社員検索 (search staff) | 1 input (search text), repeats until you decline "return to main menu?" |
| `a` | 社員追加 (add staff) | 4 inputs (number, name, ruby, dept code) + confirm y/n |
| `d` | 部署一覧 (list departments) | single step |
| `e` | 部署追加 (add department) | 2 inputs (code, name) + confirm y/n |
| `c` | AIチャット (AI chat) | 1 input (prompt), repeats until you decline "return to main menu?" |
| `x` | 終了 (exit) | saves and exits |

Any prompt also accepts `/` to cancel/step back (`UserInterface.CancelString`).

For a quick "does it still build and boot" check, run it and just exit:

```powershell
"x" | dotnet run --project Shos.StaffManager
```

To exercise a specific flow non-interactively, pipe one line per prompt,
ending with `x`. Example: list staff, then exit:

```powershell
"s`nx" | dotnet run --project Shos.StaffManager
```

Example: add a department (code 500, name テスト部), confirm, decline
"return to main menu?" (so it exits the repeat loop back to the main menu),
then exit:

```powershell
"e`n500`nテスト部`ny`nn`nx" | dotnet run --project Shos.StaffManager
```

If you don't already know the exact number of prompts a flow needs, it's
usually faster/more reliable to run `dotnet run --project Shos.StaffManager`
directly in an interactive terminal and type the sequence by hand — piped
input that runs short leaves `Console.ReadLine()` blocked forever with no
further output.

### AI chat (`c`) prerequisites

The default (non-`AZURE_OPENAI`) build talks to a local **Ollama** server at
`http://localhost:11434` with model `minimax-m3:cloud`
(`Shos.StaffManager/AI/AIAgent.cs`). If Ollama isn't running or the model
isn't available, `ChatAgent.RunAgentAsync` catches the exception and the app
prints `AI: ……` instead of crashing — so a working chat reply is a real
signal the AI stack is wired correctly, but a missing Ollama server won't
surface as a build/run failure, only as that placeholder text. Don't treat a
successful launch+exit alone as proof the AI chat works; actually send a
prompt through `c` if that's what you're verifying.

If built with the `AZURE_OPENAI` compilation symbol instead, set
`AZURE_OPENAI_ENDPOINT` and `AZURE_OPENAI_API_KEY` environment variables first
or `GetChatClient()` throws.

## 4. Running / testing the MCP server

The MCP server has no interactive console output of its own — use the MCP
Inspector to drive it and see tool results:

```bash
npx @modelcontextprotocol/inspector dotnet run --project ./Shos.StaffManager.MCPServer/Shos.StaffManager.MCPServer.csproj
```

This opens a local web UI where you can call `GetAllDepartments`,
`AddNewStaff`, etc. directly and inspect the JSON results — useful for
verifying a change to a tool method in `StaffManagerTools` without touching
the console app at all.

The project is also pre-registered for VS Code MCP in `.vscode/mcp.json`
(server id `my-mcp-server-95787fc8`), and can be pointed at from Claude
Desktop's `claude_desktop_config.json` per the README.

Remember: the console app and the MCP server each load their own in-memory
`Company` from the same file and only interact through it — running both at
once against `C:\work\FC.StaffManager.json` can lose whichever side's changes
aren't saved last.
