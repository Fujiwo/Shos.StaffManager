---
name: add-menu-command
description: Add or modify a console menu command in Shos.StaffManager — Command/SingleStepCommand subclass, mnemonic registration in CommandManager, the backward-stepping Steps contract, CommandMode, and routing input through UserInterface.Get<T> and display through View.
---

# Adding a console menu command

The console UI is a command pattern over `Company`. Everything lives in
`Shos.StaffManager/Application/Controllers.cs`, with reusable primitives in
`Shos.StaffManager/Common/ControllerBase.cs`.

## Step 1 — pick a base class

- **`SingleStepCommand<Company>`** — one action, no input sequence. Override
  `Title` and `RunFeature(Company)`. (`ShowStaffsCommand`, `ShowDepartmentsCommand`,
  `AIChatCommand`, `ExitCommand`)
- **`Command<Company>`** — multi-step wizard. Override `Title` and
  `Steps => Func<Company, bool>[]`. (`AddStaffCommand`, `AddDepartmentCommand`,
  `SearchStaffsCommand`)

## Step 2 — understand the `Steps` contract before writing steps

`DialogBox<TModel>.Show` walks `Steps` with an index:

- step returns `true` → index++ (forward); past the end → the command succeeded
- step returns `false` → index-- (**backward**); before the start → the command was cancelled

So returning `false` is *not* an error — it is how a cancelled prompt returns
the user to the previous question. Every step follows the same shape: call a
`static ...? GetXxx()` helper that returns `null` on cancel, and translate that
`null` into `false`:

```csharp
public bool SetNumber(Company company)
{
    var newNumber = GetNumber(company);
    if (newNumber is null)
        return false;
    input.Number = newNumber.Value;
    return true;
}
```

Accumulate partial input in a private `Input` class field (see `AddStaffCommand.Input`),
not on the model, and only touch `company` in the final confirm step.

## Step 3 — take input through `UserInterface.Get<T>`

`Shos.StaffManager/Common/Helpers.cs` — this is the **only** place field
validation happens. Pass `(rule, errorMessage)` tuples; it re-prompts until a
rule passes, and returns `isAvailable: false` when the user types the
`CancelString` (`/`). Parsing is generic via `TypeParser` (`T.Parse(string)`).

```csharp
static int? GetCode(Company company)
{
    var result = UserInterface.Get<int>(
        message: $"{Label.Code}を入力してください",
        [(rule: code => Department.MinimumCode <= code && code <= Department.MaximumCode,
          errorMessage: $"{Label.Code}は{Department.MinimumCode}～{Department.MaximumCode}で入力してください"),
         (rule: code => company.DepartmentList.All(department => department.Code != code),
          errorMessage: $"その {Label.Code} はすでに使われています")]
    );
    return result.isAvailable ? result.item : null;
}
```

Conventions: labels go in a private `static class Label`, range rules reuse the
`const` limits on `Department` / `Staff`, and messages are Japanese.

For a final yes/no use `ConfirmBox`:

```csharp
var confirmBox = new ConfirmBox { Title = Title };
if (confirmBox.Show(text: $"...", message: "この社員を追加しますか?")) { ... }
```

## Step 4 — choose `CommandMode`

`Mode` (default `Once`) controls `CommandManager.Run`:

- `Once` — run once, back to the main menu.
- `Repeat` — after each successful run, `ConfirmBox` asks
  "メイン メニューに戻りますか?"; `n` re-runs the command. Use for
  search / add / chat.
- `Exit` — `CommandManager.Run` returns `false` and the app saves and quits
  (`ExitCommand` only).

## Step 5 — display through `View`

Never write tables from a command. Add a `static void Show...` to
`Shos.StaffManager/Application/View.cs`, which projects to an anonymous type and
calls the `Shos.Console` `ShowTable()` extension. Keep the existing formats:
department codes `D3`, staff numbers `D4`, staff name as `名前(フリガナ)`,
department as `部署名(コード)`, and the existing `OrderBy` keys.

## Step 6 — register the mnemonic

`CommandManager`'s constructor, in `Controllers.cs`:

```csharp
menu = new() {
    Commands = [
        ('s', new ShowStaffsCommand     ()),
                ('f', new SearchStaffsCommand   ()),
                ...
                ('x', new ExitCommand           ())
    ]
};
```

Pick an unused lowercase char (`s f a d e c x` are taken); `Menu<TModel>.Select`
lowercases input and only accepts registered mnemonics. Menu order = declaration
order, and `x` 終了 should stay last.

## Verify

```bash
dotnet build
```

Then drive the new mnemonic — including a `/` cancel at each prompt to confirm
the backward stepping — per the `run` skill.
