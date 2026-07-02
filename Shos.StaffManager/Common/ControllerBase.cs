namespace Shos.StaffManager.Common.ControllerBase;

using Shos.StaffManager.Common.Helpers;

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
        const char noCharacter = 'n';
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

    /// <summary>The command table mapping mnemonics to commands</summary>
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
        var mnemonics = new string(Commands.Select(pair => pair.mnemonic).ToArray());
        var menuItemsString = MenuItemsToString();
        var separator = Separator(menuItemsString);
        var result = UserInterface.GetMnemonic($"{separator}\n{menuItemsString}\n{separator}\n{message}", mnemonics);
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
