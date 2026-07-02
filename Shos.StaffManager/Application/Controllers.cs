/// <summary>Contains controller classes for handling user commands</summary>
namespace Shos.StaffManager.Application.Controllers;

using Shos.StaffManager.AI;
using Shos.StaffManager.Application.Views;
using Shos.StaffManager.Common.ControllerBase;
using Shos.StaffManager.Common.Helpers;
using Shos.StaffManager.Models;

/// <summary>Command to display all staff members</summary>
class ShowStaffsCommand : SingleStepCommand<Company>
{
    /// <summary>Gets the title of the command</summary>
    public override string Title => "社員一覧";

    /// <summary>Executes the show staffs feature</summary>
    /// <param name="company">The company model</param>
    /// <returns>Always returns true</returns>
    protected override bool RunFeature(Company company)
    {
        View.ShowStaffs(company);
        return true;
    }
}

/// <summary>Command to search for staff members by various criteria</summary>
class SearchStaffsCommand : Command<Company>
{
    /// <summary>Gets the command mode (repeatable)</summary>
    public override CommandMode Mode => CommandMode.Repeat;

    /// <summary>Gets the title of the command</summary>
    public override string Title => "社員検索";

    /// <summary>Gets the steps for the search command</summary>
    public override Func<Company, bool>[] Steps => [SetSearchString, ShowStaffs];

    /// <summary>The current search string</summary>
    string searchString = "";

    /// <summary>Sets the search string from user input</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if search string was set successfully</returns>
    bool SetSearchString(Company company)
    {
        var newSearchString = GetSearchString();
        if (newSearchString is null)
            return false;
        searchString = newSearchString;
        return true;
    }

    /// <summary>Displays the filtered staff results</summary>
    /// <param name="company">The company model</param>
    /// <returns>Always returns true</returns>
    bool ShowStaffs(Company company)
    {
        View.ShowStaffs(company, searchString);
        return true;
    }

    /// <summary>Gets the search string from user input</summary>
    /// <returns>The search string or null if cancelled</returns>
    static string? GetSearchString()
    {
        var result = UserInterface.Get<string>(message: "検索文字列を入力してください", rules: []);
        return result.isAvailable ? result.item : null;
    }
}

/// <summary>Command to display all departments</summary>
class ShowDepartmentsCommand : SingleStepCommand<Company>
{
    /// <summary>Gets the title of the command</summary>
    public override string Title => "部署一覧";

    /// <summary>Executes the show departments feature</summary>
    /// <param name="company">The company model</param>
    /// <returns>Always returns true</returns>
    protected override bool RunFeature(Company company)
    {
        View.ShowDepartments(company);
        return true;
    }
}

/// <summary>Command to add a new staff to the company</summary>
class AddStaffCommand : Command<Company>
{
    static class Label
    {
        /// <summary>Label for staff number input</summary>
        public const string Number = "社員番号";
        /// <summary>Label for department name input</summary>
        public const string Name = "社員名";
        /// <summary>Label for department name input</summary>
        public const string Ruby = "社員名フリガナ";
        /// <summary>Label for department code input</summary>
        public const string DepartmentCode = "部署コード";
    }

    class Input
    {
        /// <summary>The staff number being added</summary>
        public int Number { get; set; }
        /// <summary>The staff name being added</summary>
        public string Name { get; set; } = "";
        /// <summary>The staff ruby being added</summary>
        public string Ruby { get; set; } = "";
        /// <summary>The department code being added</summary>
        public int DepartmentCode { get; set; }

        public Staff ToStaff(Company company)
        {
            var deparment = company.DepartmentList.FirstOrDefault(department => department.Code == DepartmentCode);
            if (deparment is null)
                throw new InvalidOperationException("Department not found");

            return new Staff(Number: Number, Name: Name, Ruby: Ruby) { Department = deparment };
        }
    }

    Input input = new();

    /// <summary>Gets the command mode (repeatable)</summary>
    public override CommandMode Mode => CommandMode.Repeat;

    /// <summary>Gets the title of the command</summary>
    public override string Title => "社員追加";

    /// <summary>Gets the steps for adding a staff</summary>
    public override Func<Company, bool>[] Steps => [SetNumber, SetName, SetRuby, SetDeparmentCode, Confirm];

    /// <summary>Sets the staff number from user input</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if code was set successfully</returns>
    public bool SetNumber(Company company)
    {
        var newCode = GetNumber(company);
        if (newCode is null)
            return false;
        input.Number = newCode.Value;
        return true;
    }

    /// <summary>Sets the department name from user input</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if name was set successfully</returns>
    public bool SetName(Company company)
    {
        var newName = GetName();
        if (newName is null)
            return false;
        input.Name = newName;
        return true;
    }

    /// <summary>Sets the department name from user input</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if name was set successfully</returns>
    public bool SetRuby(Company company)
    {
        var newRuby = GetRuby();
        if (newRuby is null)
            return false;
        input.Ruby = newRuby;
        return true;

    }

    /// <summary>Sets the department code from user input</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if code was set successfully</returns>
    public bool SetDeparmentCode(Company company)
    {
        var newCode = GetDepartmentCode(company);
        if (newCode is null)
            return false;
        input.DepartmentCode = newCode.Value;
        return true;
    }

    /// <summary>Confirms and adds the department to the company</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if department was added</returns>
    public bool Confirm(Company company)
    {
        var staff = input.ToStaff(company);
        var confirmBox = new ConfirmBox { Title = Title };

        if (confirmBox.Show(text: $"社員番号\t: {staff.Number}\n氏名\t: {staff.Name}({staff.Ruby})\n部署\t:{staff.Department.Name}({staff.Department.Code})", message: "この社員を追加しますか?")) {
            company.StaffList.Add(staff);
            return true;
        }
        return false;
    }

    /// <summary>Gets a valid staff number from user input</summary>
    /// <param name="company">The company model to check for duplicates</param>
    /// <returns>The staff number or null if cancelled</returns>
    static int? GetNumber(Company company)
    {
        var result = UserInterface.Get<int>(
            $"{Label.Number}を入力してください",
            [(rule: number => Staff.MinimumNumber <= number && number <= Staff.MaximumNumber,
                      errorMessage: $"{Label.Number}は{Staff.MinimumNumber}～{Staff.MaximumNumber}で入力してください"),
                     (rule: number => company.StaffList.All(staff => staff.Number != number),
                      errorMessage: $"その {Label.Number} はすでに使われています")]
        );
        return result.isAvailable ? result.item : null;
    }

    /// <summary>Gets a valid staff name from user input</summary>
    /// <returns>The staff name or null if cancelled</returns>
    static string? GetName()
    {
        var result = UserInterface.Get<string>(
            message: $"{Label.Name}を入力してください",
            [(rule: text => Staff.MinimumNameLength <= text.Length && text.Length <= Staff.MaximumNameLength,
                      errorMessage: $"{Label.Name}は{Staff.MinimumNameLength}文字～{Staff.MaximumNameLength}文字で入力してください")]
        );
        return result.isAvailable ? result.item : null;
    }

    /// <summary>Gets a valid staff ruby (phonetic reading) from user input</summary>
    /// <returns>The staff ruby or null if cancelled</returns>
    static string? GetRuby()
    {
        var result = UserInterface.Get<string>(
            message: $"{Label.Ruby}を入力してください",
            [(rule: text => Staff.MinimumNameLength <= text.Length && text.Length <= Staff.MaximumNameLength,
                      errorMessage: $"{Label.Ruby}は{Staff.MinimumNameLength}文字～{Staff.MaximumNameLength}文字で入力してください")]
        );
        return result.isAvailable ? result.item : null;
    }

    /// <summary>Gets a valid department code from user input</summary>
    /// <param name="company">The company model to check for duplicates</param>
    /// <returns>The department code or null if cancelled</returns>
    static int? GetDepartmentCode(Company company)
    {
        var result = UserInterface.Get<int>(
            $"{Label.DepartmentCode}を入力してください",
            [(rule: code => company.DepartmentList.Any(department => department.Code == code),
                      errorMessage: $"その {Label.DepartmentCode} は存在しません")]
        );
        return result.isAvailable ? result.item : null;
    }
}

/// <summary>Command to add a new department to the company</summary>
class AddDepartmentCommand : Command<Company>
{
    static class Label
    {
        /// <summary>Label for department code input</summary>
        public const string Code = "部署コード";
        /// <summary>Label for department name input</summary>
        public const string Name = "部署名";
    }

    class Input
    {
        /// <summary>The department code being added</summary>
        public int Code { get; set; }
        /// <summary>The department name being added</summary>
        public string Name { get; set; } = "";

        public Department ToDepartment()
            => new Department(Code: Code) { Name = Name };
    }

    Input input = new();

    /// <summary>Gets the command mode (repeatable)</summary>
    public override CommandMode Mode => CommandMode.Repeat;

    /// <summary>Gets the title of the command</summary>
    public override string Title => "部署追加";

    /// <summary>Gets the steps for adding a department</summary>
    public override Func<Company, bool>[] Steps => [SetCode, SetName, Confirm];

    /// <summary>Sets the department code from user input</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if code was set successfully</returns>
    public bool SetCode(Company company)
    {
        var newCode = GetCode(company);
        if (newCode is null)
            return false;
        input.Code = newCode.Value;
        return true;
    }

    /// <summary>Sets the department name from user input</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if name was set successfully</returns>
    public bool SetName(Company company)
    {
        var newName = GetName();
        if (newName is null)
            return false;
        input.Name = newName;
        return true;
    }

    /// <summary>Confirms and adds the department to the company</summary>
    /// <param name="company">The company model</param>
    /// <returns>True if department was added</returns>
    public bool Confirm(Company company)
    {
        var department = input.ToDepartment();
        var confirmBox = new ConfirmBox { Title = Title };

        if (confirmBox.Show(text: $"部署コード\t: {department.Code}\n部署名\t:{department.Name}", message: "この部署を追加しますか?")) {
            company.DepartmentList.Add(department);
            return true;
        }
        return false;
    }

    /// <summary>Gets a valid department code from user input</summary>
    /// <param name="company">The company model to check for duplicates</param>
    /// <returns>The department code or null if cancelled</returns>
    static int? GetCode(Company company)
    {
        var result = UserInterface.Get<int>(
            $"{Label.Code}を入力してください",
            [(rule: code => Department.MinimumCode <= code && code <= Department.MaximumCode,
                      errorMessage: $"{Label.Code}は{Department.MinimumCode}～{Department.MaximumCode}で入力してください"),
                     (rule: code => company.DepartmentList.All(department => department.Code != code),
                      errorMessage: $"その {Label.Code} はすでに使われています")]
        );
        return result.isAvailable ? result.item : null;
    }

    /// <summary>Gets a valid department name from user input</summary>
    /// <returns>The department name or null if cancelled</returns>
    static string? GetName()
    {
        var result = UserInterface.Get<string>(
            message: $"{Label.Name}を入力してください",
            [(rule: text => Department.MinimumNameLength <= text.Length && text.Length <= Department.MaximumNameLength,
                      errorMessage: $"{Label.Name}は{Department.MinimumNameLength}文字～{Department.MaximumNameLength}文字で入力してください")]
        );
        return result.isAvailable ? result.item : null;
    }
}

/// <summary>Command to exit the application</summary>
class ExitCommand : SingleStepCommand<Company>
{
    /// <summary>Gets the command mode (exit)</summary>
    public override CommandMode Mode => CommandMode.Exit;

    /// <summary>Gets the title of the command</summary>
    public override string Title => "終了";

    /// <summary>Executes the exit feature</summary>
    /// <param name="company">The company model</param>
    /// <returns>Always returns false to exit</returns>
    protected override bool RunFeature(Company company) => false;
}

/// <summary>Command to start AI chat</summary>
class AIChatCommand : SingleStepCommand<Company>
{
    MyChatAgent chatAgent = new();

    /// <summary>Gets the command mode (exit)</summary>
    public override CommandMode Mode => CommandMode.Repeat;

    /// <summary>Gets the title of the command</summary>
    public override string Title => "AIチャット";

    /// <summary>Executes the exit feature</summary>
    /// <param name="company">The company model</param>
    /// <returns>Always returns false to exit</returns>
    protected override bool RunFeature(Company company)
    {
        var prompt = GetPrompt();
        if (prompt is null)
            return false;
        var response = GetResponse(prompt);
        UserInterface.Show($"AI: {response}");
        return true;
    }

    string GetResponse(string prompt)
        => chatAgent.GetResponseAsync(prompt).Result;

    static string? GetPrompt()
    {
        const string label = "プロンプト";
        var result = UserInterface.Get<string>(
            message: $"{label}を入力してください",
            [(rule: text => !string.IsNullOrWhiteSpace(text),
                     errorMessage: $"{label}は空白以外の文字を入力してください")]
        );
        return result.isAvailable ? result.item : null;
    }
}

/// <summary>Manages the execution of commands through a menu system</summary>
class CommandManager
{
    /// <summary>The menu for command selection</summary>
    readonly Menu<Company> menu;

    /// <summary>Initializes a new instance of the CommandManager class</summary>
    public CommandManager()
    {
        menu = new() {
            Commands = [
                ('s', new ShowStaffsCommand     ()),
                        ('f', new SearchStaffsCommand   ()),
                        ('a', new AddStaffCommand       ()),
                        ('d', new ShowDepartmentsCommand()),
                        ('e', new AddDepartmentCommand  ()),
                        ('c', new AIChatCommand         ()),
                        ('x', new ExitCommand           ())
            ]
        };
    }

    /// <summary>Runs the command manager, allowing user to select and execute commands</summary>
    /// <param name="model">The company model</param>
    /// <returns>True to continue running, false to exit</returns>
    public bool Run(Company model)
    {
        Toolbox.Company = model;

        var command = Select();
        if (command is null)
            return false;
        while (command.Run(model) && command.Mode == Command<Company>.CommandMode.Repeat) {
            var confirmBox = new ConfirmBox { Title = command.Title };
            if (confirmBox.Show(text: "", message: "メイン メニューに戻りますか?"))
                break;
        }
        return command.Mode != Command<Company>.CommandMode.Exit;
    }

    /// <summary>Allows user to select a command from the menu</summary>
    /// <returns>The selected command or null if cancelled</returns>
    Command<Company>? Select() => menu.Select("操作を選択してください");
}
