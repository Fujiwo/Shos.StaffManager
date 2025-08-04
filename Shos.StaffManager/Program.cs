namespace StaffManager
{
    namespace Common
    {
        namespace Helpers
        {
            using System.Globalization;
            using System.Reflection;
            using System.Text;
            using System.Text.RegularExpressions;

            /// <summary>Extension methods for IEnumerable</summary>
            public static class EnumerableExtensions
            {
                // This would provide a ForEach extension method for IEnumerable collections
                public static void ForEach<TElement>(this IEnumerable<TElement> @this, Action<TElement> action)
                {
                    foreach (TElement item in @this)
                        action(item);
                }
            }

            /// <summary>Extension methods for string operations</summary>
            public static class StringExtensions
            {
                /// <summary>Calculates the display width of a string considering full-width Japanese characters</summary>
                /// <param name="this">The string to measure</param>
                /// <returns>The display width (full-width characters count as 2, half-width as 1)</returns>
                public static int Width(this string @this) =>
                    @this.Select(character => IsZenkaku(character) ? 2 : 1).Sum();

                /// <summary>Determines if a character is full-width (Zenkaku)</summary>
                /// <param name="this">The character to check</param>
                /// <returns>True if the character is full-width</returns>
                public static bool IsZenkaku(this char @this) => new string(@this, 1).IsZenkaku();
                
                /// <summary>Determines if a string contains full-width (Zenkaku) characters</summary>
                /// <param name="this">The string to check</param>
                /// <returns>True if the string contains full-width characters</returns>
                public static bool IsZenkaku(this string @this)
                     // Regex pattern matches any character NOT in the range of ASCII (x01-x7E) or half-width katakana (ｦ-ﾟ)
                     => Regex.IsMatch(@this, "[^\x01-\x7Eｦ-ﾟ]");
            }

            /// <summary>Generic type parser utility for converting strings to various types</summary>
            static class TypeParser
            {
                /// <summary>Parses a string to the specified type</summary>
                /// <typeparam name="T">The target type</typeparam>
                /// <param name="this">The string to parse</param>
                /// <returns>The parsed value or null if parsing fails</returns>
                public static T? Parse<T>(this string @this)
                    => (T?)typeof(T).Parse(@this);

                /// <summary>Attempts to parse a string to the specified type</summary>
                /// <typeparam name="T">The target type</typeparam>
                /// <param name="this">The string to parse</param>
                /// <returns>A tuple containing whether parsing succeeded and the result</returns>
                public static (bool canParse, T? result) TryParse<T>(this string @this)
                {
                    var (canParse, result) = typeof(T).TryParse(@this);
                    return (canParse, (T?)result);
                }

                /// <summary>Parses a string to the specified type using reflection</summary>
                /// <param name="this">The target type</param>
                /// <param name="text">The string to parse</param>
                /// <returns>The parsed value or null</returns>
                /// <exception cref="ArgumentNullException">Thrown when parameters are null</exception>
                public static object? Parse(this Type @this, string text)
                {
                    ArgumentNullException.ThrowIfNull(@this);
                    ArgumentNullException.ThrowIfNull(text);

                    // Get the underlying type if this is a nullable type
                    var targetType = Nullable.GetUnderlyingType(@this) ?? @this;

                    // Return null for empty strings on nullable types
                    if (string.IsNullOrEmpty(text) && @this != targetType)
                        return null;

                    // Try to find a static Parse method that takes a string parameter
                    var parseMethod = targetType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static,
                                                           null, [typeof(string)], null);

                    // Use the Parse method if available
                    if (parseMethod is not null)
                        return parseMethod.Invoke(null, [text]);

                    // Fall back to Convert.ChangeType as a last resort
                    return Convert.ChangeType(text, targetType, CultureInfo.InvariantCulture);
                }

                /// <summary>Attempts to parse a string to the specified type using reflection</summary>
                /// <param name="this">The target type</param>
                /// <param name="text">The string to parse</param>
                /// <returns>A tuple containing whether parsing succeeded and the result</returns>
                public static (bool canParse, object? result) TryParse(this Type @this, string text)
                {
                    try {
                        var result = Parse(@this, text);
                        return (result is not null, result);
                    } catch (Exception) {
                        return (false, null);
                    }
                }
            }

            /// <summary>Utility class for temporarily changing console colors with automatic restoration</summary>
            class ConsoleColorSetter : IDisposable
            {
                /// <summary>Stores the original foreground color to restore later</summary>
                ConsoleColor oldForeground = Console.ForegroundColor;
                /// <summary>Stores the original background color to restore later</summary>
                ConsoleColor oldBackground = Console.BackgroundColor;

#pragma warning disable CA1822
                /// <summary>Sets the console foreground color</summary>
                public ConsoleColor Foreground { set => Console.ForegroundColor = value; }
                
                /// <summary>Sets the console background color</summary>
                public ConsoleColor Background { set => Console.BackgroundColor = value; }
#pragma warning restore CA1822

                /// <summary>Restores the original console colors</summary>
                public void Dispose()
                    => (Console.ForegroundColor, Console.BackgroundColor) = (oldForeground, oldBackground);
            }

            /// <summary>Provides user interface utilities for console input/output operations</summary>
            static class UserInterface
            {
                /// <summary>The string used to cancel input operations</summary>
                public static string CancelString { get; set; } = "/";
                /// <summary>The header text displayed before error messages</summary>
                public static string ErrorHeader  { get; set; } = "[エラー]";

                /// <summary>Gets user input with validation rules</summary>
                /// <typeparam name="T">The type of input to get</typeparam>
                /// <param name="message">The prompt message</param>
                /// <param name="rules">Validation rules with error messages</param>
                /// <returns>A tuple indicating if input is available and the parsed value</returns>
                public static (bool isAvailable, T? item) Get<T>(string message, IEnumerable<(Func<T, bool> rule, string errorMessage)> rules)
                {
                    for (; ;) {
                        ShowPrompt(message);
                        var line = GetLine();
                        if (line == CancelString)
                            return (false, default);
                        var (canParse, value) = line.TryParse<T>();
                        if (!canParse || value is null)
                            continue;

                        var item = (T)value;
                        var error = rules.FirstOrDefault(rule => !rule.rule(item));
                        if (error.rule is null)
                            return (true, item);
                        else
                            ShowError(error.errorMessage);
                    }
                }

                /// <summary>Gets a single character mnemonic from user input</summary>
                /// <param name="message">The prompt message</param>
                /// <param name="mnemonics">Valid mnemonic characters</param>
                /// <returns>A tuple indicating if input is available and the selected mnemonic</returns>
                public static (bool isAvailable, char mnemonic) GetMnemonic(string message, string mnemonics)
                {
                    for (; ;) {
                        ShowPrompt(message);
                        var line = GetLine();
                        if (line == CancelString)
                            return (false, default);
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        var mnemonic = char.ToLower(line[0]);
                        if (mnemonics.Contains(mnemonic))
                            return (true, mnemonic);
                    }
                }

                /// <summary>Displays a prompt message</summary>
                /// <param name="message">The message to display</param>
                public static void ShowPrompt(string message) => Console.Write($"{message}:");

                /// <summary>Displays an error message in red color</summary>
                /// <param name="message">The error message to display</param>
                public static void ShowError(string message)
                {
                    using var consoleColorSetter = new ConsoleColorSetter { Foreground = ConsoleColor.Red };
                    Show($"{ErrorHeader} {message}");
                }

                /// <summary>Displays a message to the console</summary>
                /// <param name="message">The message to display</param>
                public static void Show(string message = "") => Console.WriteLine(message);
                
                /// <summary>Displays a separator line</summary>
                /// <param name="separatorCharacter">The character to use for the separator</param>
                /// <param name="length">The length of the separator</param>
                public static void ShowSeparator(char separatorCharacter, int length) => Show(Separator(separatorCharacter, length));
                
                /// <summary>Creates a separator string</summary>
                /// <param name="separatorCharacter">The character to repeat</param>
                /// <param name="length">The number of characters</param>
                /// <returns>A string of repeated characters</returns>
                public static string Separator(char separatorCharacter, int length) => new string(separatorCharacter, length);

                /// <summary>Gets a normalized line of input from the console</summary>
                /// <returns>The trimmed and normalized input string</returns>
                static string GetLine() => Console.ReadLine()?.Trim()?.Normalize(NormalizationForm.FormKC) ?? "";
            }
        }

        namespace ControllersBase
        {
            using Common.Helpers;

            /// <summary>Base class for window-like UI components</summary>
            class Window
            {
                public static char separatorCharacter { get; set; } = '-';

                /// <summary>Gets or sets the title of the window</summary>
                public required string Title { protected get; set; }

                /// <summary>Displays the title bar with separators</summary>
                /// <param name="separatorLength">The length of the separator lines</param>
                protected void ShowTitleBar(int separatorLength)
                {
                    UserInterface.Show();
                    ShowSeparator(separatorLength);
                    UserInterface.Show($"【{Title}】");
                    ShowSeparator(separatorLength);
                }

                /// <summary>Displays a separator line</summary>
                /// <param name="separatorLength">The length of the separator</param>
                protected static void ShowSeparator(int separatorLength)
                    => UserInterface.ShowSeparator(separatorCharacter: separatorCharacter, separatorLength);

            }

            /// <summary>A confirmation dialog box that prompts the user for yes/no input</summary>
            class ConfirmBox : Window
            {
                /// <summary>Shows the confirmation dialog and waits for user response</summary>
                /// <param name="text">The main text content to display in the dialog</param>
                /// <param name="message">The confirmation question to ask the user</param>
                /// <returns>True if user confirms (presses 'y'), false if user declines or cancels</returns>
                public bool Show(string text, string message)
                {
                    // Calculate dialog width based on content plus margin for better display
                    const int marginWidth = 8;
                    var width = text.Split('\n')
                                    .Concat(new string[] { Title, message })
                                    .Select(text => text.Length)
                                    .Max()
                                + marginWidth;
 
                    ShowTitleBar(separatorLength: width);
                    UserInterface.Show($"{text}");
                    ShowSeparator(separatorLength: width);

                    // Define characters for yes/no confirmation
                    const char yesCharacter = 'y';
                    const char noCharacter  = 'n';
                    var result = UserInterface.GetMnemonic($"{message} ({yesCharacter}/{noCharacter})", $"{yesCharacter}{noCharacter}");
                    return result.isAvailable && result.mnemonic == yesCharacter;
                }
            }

            /// <summary>A multi-step dialog box for user interactions</summary>
            /// <typeparam name="TModel">The model type for the dialog</typeparam>
            class DialogBox<TModel> : Window
            {
                /// <summary>Gets or sets the steps to execute in the dialog</summary>
                public required Func<TModel, bool>[] Steps { get; set; }

                /// <summary>Shows the dialog and executes the steps</summary>
                /// <param name="model">The model to pass to each step</param>
                /// <returns>True if all steps completed successfully</returns>
                public bool Show(TModel model)
                {
                    ShowTitleBar(separatorLength: 80);

                    for (var index = 0; ;) {
                        if (Steps[index](model)) {
                            index++;
                            if (index >= Steps.Length)
                                return true;
                        } else {
                            index--;
                            if (index < 0)
                                return false;
                        }
                    }
                }
            }

            /// <summary>Base class for executable commands</summary>
            /// <typeparam name="TModel">The model type for the command</typeparam>
            abstract class Command<TModel>
            {
                /// <summary>Defines the execution mode of the command</summary>
                public enum CommandMode { Exit, Once, Repeat }

                /// <summary>Gets the execution mode of the command</summary>
                public virtual CommandMode Mode => CommandMode.Once;

                /// <summary>Gets the title of the command</summary>
                public abstract string Title { get; }
                
                /// <summary>Gets the steps to execute for this command</summary>
                public virtual Func<TModel, bool>[] Steps => [];

                /// <summary>Runs the command with the specified model</summary>
                /// <param name="model">The model to pass to the command</param>
                /// <returns>True if the command executed successfully</returns>
                public bool Run(TModel model)
                    => new DialogBox<TModel> { Title = Title, Steps = Steps }.Show(model);
            }

            /// <summary>Base class for commands that execute a single step</summary>
            /// <typeparam name="TModel">The model type for the command</typeparam>
            abstract class SingleStepCommand<TModel> : Command<TModel>
            {
                /// <summary>Gets the single step to execute</summary>
                public override Func<TModel, bool>[] Steps => [RunFeature];

                /// <summary>Executes the main feature of the command</summary>
                /// <param name="model">The model to operate on</param>
                /// <returns>True if the feature executed successfully</returns>
                protected abstract bool RunFeature(TModel model);
            }

            /// <summary>A menu system for selecting and executing commands</summary>
            /// <typeparam name="TModel">The model type for commands</typeparam>
            class Menu<TModel>
            {
                /// <summary>The character used to draw separator lines in the menu</summary>
                const char separatorCharacter = '-';

                /// <summary>Gets or sets the command table mapping mnemonics to commands</summary>
                Dictionary<char, Command<TModel>> commandTable = null!;
                IEnumerable<(char mnemonic, Command<TModel> command)> commands = null!;

                /// <summary>Gets or sets the mnemonics and commands</summary>
                public required IEnumerable<(char mnemonic, Command<TModel> command)> Commands
                {
                    private get => commands;
                    set {
                        commands = value;
                        commandTable = new();
                        value.ForEach(pair => commandTable[pair.mnemonic] = pair.command);
                    }
                }

                /// <summary>Displays the menu and allows user to select a command</summary>
                /// <param name="message">The message to display for selection</param>
                /// <returns>The selected command or null if cancelled</returns>
                public Command<TModel>? Select(string message)
                {
                    var mnemonics       = new string(Commands.Select(pair => pair.mnemonic).ToArray());
                    var menuItemsString = MenuItemsToString();
                    var separator       = Separator(menuItemsString);
                    var result          = UserInterface.GetMnemonic($"{separator}\n{menuItemsString}\n{separator}\n{message}", mnemonics);
                    return result.isAvailable ? commandTable[result.mnemonic] : null;
                }

                /// <summary>Converts menu items to a display string</summary>
                /// <returns>A formatted string of menu items</returns>
                string MenuItemsToString()
                    => string.Join(", ", Commands.Select(pair => $"({pair.mnemonic}){pair.command.Title}").ToArray());
                
                /// <summary>Creates a separator string based on menu items width</summary>
                /// <param name="menuItemsString">The menu items string to measure</param>
                /// <returns>A separator string</returns>
                static string Separator(string menuItemsString)
                    => UserInterface.Separator(separatorCharacter, menuItemsString.Width());
            }
        }
    }

    /// <summary>Main application namespace containing models, views, and controllers</summary>
    namespace Application
    {
        using Shos.StaffManager.Models;
        using StaffManager.Common.Helpers;

        /// <summary>Contains view classes for displaying data</summary>
        namespace Views
        {
            using Shos.Console;

            /// <summary>Provides methods for displaying company data in table format</summary>
            class View
            {
                /// <summary>Displays a collection of departments in table format</summary>
                /// <param name="departments">The departments to display</param>
                public static void Show(IEnumerable<Department> departments)
                    => departments.OrderBy(department => department.Code)
                                  .Select(department => new {
                                      Code = $"{department.Code:D3}",
                                      Name = department.Name
                                  })
                                  .ShowTable();

                /// <summary>Displays a collection of staff members in table format</summary>
                /// <param name="staffs">The staff members to display</param>
                public static void Show(IEnumerable<Staff> staffs)
                    => staffs.OrderBy(staff => staff.Department.Code)
                             .ThenBy(staff => staff.Number)
                             .Select(staff => new {
                                 Number = $"{staff.Number:D4}",
                                 Name = $"{staff.Name}({staff.Ruby})",
                                 Department = $"{staff.Department.Name}({staff.Department.Code})"
                             })
                             .ShowTable();

                /// <summary>Displays all departments in the company</summary>
                /// <param name="company">The company model</param>
                public static void ShowDepartments(Company company) => Show(company.DepartmentList);

                /// <summary>Displays all staff members in the company</summary>
                /// <param name="company">The company model</param>
                public static void ShowStaffs(Company company) => Show(company.StaffList);

                /// <summary>Displays staff members filtered by search text</summary>
                /// <param name="company">The company model</param>
                /// <param name="searchText">The text to search for in names, numbers, or departments</param>
                public static void ShowStaffs(Company company, string searchText)
                    => Show(company.GetStaffs(searchText));
            }
        }

        /// <summary>Contains controller classes for handling user commands</summary>
        namespace Controllers
        {
            using Common.ControllersBase;
            using Common.Helpers;
            using Views;

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
                    public const string Number         = "社員番号";
                    /// <summary>Label for department name input</summary>
                    public const string Name           = "社員名";
                    /// <summary>Label for department name input</summary>
                    public const string Ruby           = "社員名フリガナ";
                    /// <summary>Label for department code input</summary>
                    public const string DepartmentCode = "部署コード";
                }

                class Input
                {
                    /// <summary>The staff number being added</summary>
                    public int    Number         { get; set; }
                    /// <summary>The staff name being added</summary>
                    public string Name           { get; set; } = "";
                    /// <summary>The staff ruby being added</summary>
                    public string Ruby           { get; set; } = "";
                    /// <summary>The department code being added</summary>
                    public int    DepartmentCode { get; set; }

                    public Staff ToStaff(Company company)
                    {
                        var deparment = company.DepartmentList.FirstOrDefault(department => department.Code == DepartmentCode);
                        if (deparment is null)
                            throw new InvalidOperationException("Department not found");

                        return new Staff(Number: Number, Name: Name, Ruby: Ruby, Department: deparment);
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
                    public int    Code { get; set; }
                    /// <summary>The department name being added</summary>
                    public string Name { get; set; } = "";

                    public Department ToDepartment()
                        => new Department(Code: Code, Name: Name);
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
                            ('x', new ExitCommand           ())
                        ]
                    };
                }

                /// <summary>Runs the command manager, allowing user to select and execute commands</summary>
                /// <param name="model">The company model</param>
                /// <returns>True to continue running, false to exit</returns>
                public bool Run(Company model)
                {
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
        }

        /// <summary>Main program class that orchestrates the staff management application</summary>
        class Program
        {
            /// <summary>The file path for saving/loading company data</summary>
            const string dataFilePath = "FC.StaffManager.json";
            /// <summary>The name of the application</summary>
            const string applicationName = "FC.StaffManager";

            /// <summary>The company data model</summary>
            Company company = null!;
            /// <summary>The command manager for handling user operations</summary>
            Controllers.CommandManager operationManager = new();

            /// <summary>Runs the main application loop</summary>
            void Run()
            {
                if (!Load())
                    return;

                ShowTitle();
                while (operationManager.Run(company))
                    ;
                Save();
            }

            bool Load()
            {
                try {
                    company = Company.Load(dataFilePath);
                    return true;
                } catch (Exception ex) {
                    UserInterface.ShowError($"データが読み込めませんでした。\n{ex.Message}");
                    return false;
                }
            }

            bool Save()
            {
                try {
                    company.Save(dataFilePath);
                    return true;
                } catch (Exception ex) {
                    UserInterface.ShowError($"データが書き込めませんでした。\n{ex.Message}");
                    return false;
                }
            }

            /// <summary>Displays the application title</summary>
            static void ShowTitle() => UserInterface.Show($"<<{applicationName}>>");
            
            /// <summary>The main entry point of the application</summary>
            /// <param name="args">Command line arguments</param>
            static void Main(string[] args) => new Program().Run();
        }
    }
}
