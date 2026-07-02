namespace Shos.StaffManager.Common.Helpers;

using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System;

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
    public static string ErrorHeader { get; set; } = "[エラー]";

    /// <summary>Gets user input with validation rules</summary>
    /// <typeparam name="T">The type of input to get</typeparam>
    /// <param name="message">The prompt message</param>
    /// <param name="rules">Validation rules with error messages</param>
    /// <returns>A tuple indicating if input is available and the parsed value</returns>
    public static (bool isAvailable, T? item) Get<T>(string message, IEnumerable<(Func<T, bool> rule, string errorMessage)> rules)
    {
        for (; ; ) {
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
        for (; ; ) {
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

