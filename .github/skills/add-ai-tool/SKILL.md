---
name: add-ai-tool
description: Add or change an AI-callable operation in Shos.StaffManager. Covers the three places that must stay in sync by hand — Company/Models, the in-app Toolbox (plus its AIFunctionFactory registration), and the MCP server's StaffManagerTools — and the save/exception rules each side follows.
---

# Adding an AI-callable operation

There are **two independent tool surfaces** that are hand-kept in sync (they
share no code):

| Surface | File | Consumer |
|---------|------|----------|
| In-app agent tools | `Shos.StaffManager/AI/ToolBox.cs` (`Toolbox`) | the `c` AIチャット menu command |
| MCP server tools | `Shos.StaffManager.MCPServer/Program.cs` (`StaffManagerTools`) | Claude Desktop / VS Code / MCP Inspector |

Adding a tool to only one of them is the most common mistake in this repo.

## Step 1 — implement the domain operation once, in Models

Put the real logic in `Shos.StaffManager.Models/Models.cs` (`Company`,
`DepartmentList`, or `StaffList`) — never in a tool method. Domain invariants
belong here too (e.g. `Company.RemoveDepartment` returns `false` instead of
removing a department that still has staff).

Follow the existing shape: return `bool` for "found and applied", use
`FirstOrDefault` lookups by `Code` / `Number`, and reuse the `const` limits on
`Department` / `Staff` (`MinimumCode`, `MaximumCode`, `MinimumNumber`,
`MaximumNumber`, `MinimumNameLength`, `MaximumNameLength`) rather than
hardcoding ranges.

`DepartmentList` / `StaffList` wrap `UndoRedoList<T>`, so mutate through their
own methods (`Add`, `Remove`, `ChangeDepartmentName`, `ChangeDepartment`) to
keep undo history coherent.

## Step 2 — expose it in the in-app `Toolbox`

`Shos.StaffManager/AI/ToolBox.cs`:

```csharp
[Description("Change a department's name by department code")]
public static bool ChangeDepartmentNameByCode(int departmentCode, string newName)
{
    try {
        if (Company is not null && Company.ChangeDepartmentName(departmentCode, newName))
            return true;
    } catch {
    }
    return false;
}
```

Rules specific to this surface:

- `Toolbox.Company` is `static Company?`, assigned by `CommandManager.Run` on
  every menu iteration — always null-check it and return `[]` / `false` when null.
- Every method needs a `[Description(...)]` (English) — that text is what the
  model sees.
- Swallow exceptions and return `bool`; never throw out of a tool.
- **Do not save here.** The console app persists only on clean exit (`x`).

## Step 3 — register it with the agent (easy to forget)

A method in `Toolbox` is *not* callable until it is added to `GetCadTools()`
inside `MyChatAgent.GetToolsAsync` in `Shos.StaffManager/AI/AIAgent.cs`:

```csharp
static IEnumerable<AITool> GetCadTools()
    => [AIFunctionFactory.Create(Toolbox.GetAllDepartments             ),
        ...
        AIFunctionFactory.Create(Toolbox.ChangeStaffsDepartmentByNumber)];
```

Keep the closing-paren alignment of the existing entries.

## Step 4 — mirror it in the MCP server

`Shos.StaffManager.MCPServer/Program.cs`, in `StaffManagerTools`:

```csharp
[McpServerTool, Description("Change a department's name by department code")]
public static bool ChangeDepartmentNameByCode(int departmentCode, string newName)
{
    try {
        if (company is not null && company.ChangeDepartmentName(departmentCode, newName)) {
            Save();
            return true;
        }
    } catch {
    }
    return false;
}
```

Differences from the in-app version:

- `[McpServerTool]` in addition to `[Description]`.
- `company` is a non-nullable `static` field loaded once at class init.
- **Every mutating tool calls `Save()` immediately** (`company.Save(dataFilePath)`).
  Read-only tools do not.

## Naming

Keep tool names identical between the two surfaces, and **never rename an
existing tool** — MCP clients are configured against the current names.
That includes the inconsistent ones already shipped:

- `RemoveStaffbyNumber` (lowercase `b`) — leave as-is.
- `GetAllStaffs` / `SearchStaffs` — pluralized as "staffs" throughout.

`ShowAllDepartments` / `ShowAllStaffs` exist **only** in `Toolbox` (they write
to the console via `View`); do not add console-writing tools to the MCP server,
whose stdout is the protocol transport.

## Verify

```bash
dotnet build
```

Then exercise the tool with the MCP Inspector (fastest, no LLM needed) and, if
you touched `Toolbox`, through the `c` menu command as well — see the `run`
skill for both, including the `C:\work\FC.StaffManager.json` data-file setup.
