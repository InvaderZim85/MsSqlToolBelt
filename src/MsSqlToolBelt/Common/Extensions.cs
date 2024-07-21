using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using ZimLabs.Mapper;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides several helper functions
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Checks if the char is a number
    /// </summary>
    /// <param name="value">The char</param>
    /// <returns><see langword="true"/> when the char is a number, otherwise <see langword="false"/></returns>
    public static bool IsNumeric(this char value)
    {
        return int.TryParse(value.ToString(), out _);
    }

    /// <summary>
    /// Converts the first char of a string to lower case
    /// </summary>
    /// <param name="value">The original value</param>
    /// <returns>The converted string</returns>
    public static string FirstCharToLower(this string value)
    {
        return ChangeFirstChart(value, false);
    }

    /// <summary>
    /// Converts the first char of a string to upper case
    /// </summary>
    /// <param name="value">The original value</param>
    /// <returns>The converted string</returns>
    public static string FirstChatToUpper(this string value)
    {
        return ChangeFirstChart(value, true);
    }

    /// <summary>
    /// Changes the casing of the first char
    /// </summary>
    /// <param name="value">The original value</param>
    /// <param name="upper"><see langword="true"/> to convert the first char to upper, <see langword="false"/> to convert the first char to lower</param>
    /// <returns>The converted string</returns>
    private static string ChangeFirstChart(string value, bool upper)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return upper
            ? $"{value[..1].ToUpper()}{value[1..]}"
            : $"{value[..1].ToLower()}{value[1..]}";
    }

    /// <summary>
    /// Converts the value into a readable size
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="divider">The divider (optional)</param>
    /// <returns>The converted size</returns>
    public static string ConvertSize(this long value, int divider = 1024)
    {
        return value switch
        {
            _ when value < divider => $"{value:N0} Bytes",
            _ when value >= divider && value < Math.Pow(divider, 2) => $"{value / divider:N2} KB",
            _ when value >= Math.Pow(divider, 2) && value < Math.Pow(divider, 3) =>
                $"{value / Math.Pow(divider, 2):N2} MB",
            _ when value >= Math.Pow(divider, 3) && value <= Math.Pow(divider, 4) => $"{value / Math.Pow(divider, 3):N2} GB",
            _ when value >= Math.Pow(divider, 4) => $"{value / Math.Pow(divider, 4)} TB",
            _ => value.ToString("N0")
        };
    }

    /// <summary>
    /// Creates a clone of the desired entry
    /// </summary>
    /// <typeparam name="T">The type of the entry</typeparam>
    /// <param name="entry">The entry which should be cloned</param>
    /// <returns>The cloned entry</returns>
    public static T Clone<T>(this T entry) where T : class, new()
    {
        return Mapper.CreateAndMap<T, T>(entry);
    }

    /// <summary>
    /// Converts the value into a decimal value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The decimal value</returns>
    public static decimal ToDecimal(this object? value)
    {
        return decimal.TryParse(value?.ToString() ?? string.Empty, out var result) ? result : 0;
    }

    /// <summary>
    /// Converts the <see cref="IEnumerable{T}"/> into a <see cref="ObservableCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="source">The source list</param>
    /// <returns>The observable collection</returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    }

    /// <summary>
    /// Formats the time span into a readable string
    /// </summary>
    /// <param name="value">The original value</param>
    /// <param name="showDays"><see langword="true"/> to show the days, otherwise false</param>
    /// <returns>The formatted time span</returns>
    public static string ToFormattedString(this TimeSpan value, bool showDays = false)
    {
        return showDays
            ? $"{value.TotalDays:N0}d {value.Hours:00}:{value.Minutes:00}:{value.Seconds:00}"
            : $"{value.TotalHours:#00}:{value.Minutes:00}:{value.Seconds:00}";
    }

    /// <summary>
    /// Converts the color to a HEX value (for example <i>Green</i> > <c>#FF00FF00</c>)
    /// </summary>
    /// <param name="color">The color</param>
    /// <returns>The HEX value of the color</returns>
    public static string ToHex(this Color? color)
    {
        return color == null
            ? string.Empty
            : color.Value.ToHex();
    }

    /// <summary>
    /// Converts the color to a HEX value (for example <i>Green</i> > <c>#FF00FF00</c>)
    /// </summary>
    /// <param name="color">The color</param>
    /// <returns>The HEX value of the color</returns>
    public static string ToHex(this Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// Converts the bool into a string (<see langword="true"/> = <c>1</c>, <see langword="false"/> = <c>0</c>)
    /// </summary>
    /// <param name="value">The bool value</param>
    /// <returns>The string value according to the given input value</returns>
    public static string BoolToString(this bool value)
    {
        return value ? "1" : "0";
    }

    /// <summary>
    /// Converts a string entry to a bool (<c>"1"</c> = <see langword="true"/>, <c>"0"</c> = <see langword="false"/>)
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The bool value according to the given input value</returns>
    public static bool StringToBool(this string value)
    {
        return value.Contains(';')
            ? value.StringListToBool().FirstOrDefault()
            : value.Equals("1");
    }

    /// <summary>
    /// Converts a char into a bool (<c>1</c> = <see langword="true"/>, <c>0</c> = <see langword="false" />)
    /// </summary>
    /// <param name="value">The char</param>
    /// <returns>The bool value according to the given input value</returns>
    public static bool ToBool(this char value)
    {
        return value.Equals('1');
    }

    /// <summary>
    /// Converts a comma separated string list into a bool list. For example: <c>"1;0;1"</c> = <see langword="true"/>, <see langword="false"/>, <see langword="true"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The list with the values</returns>
    public static List<bool> StringListToBool(this string value)
    {
        return value.Contains(';')
            ? value.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(s => s.StringToBool()).ToList()
            : [value.StringToBool()];
    }

    /// <summary>
    /// Converts the <see langword="bool"/> value into a <see cref="Visibility"/> value
    /// </summary>
    /// <param name="value">The original value</param>
    /// <returns>The visibility value</returns>
    public static Visibility ToVisibility(this bool value)
    {
        return value ? Visibility.Visible : Visibility.Collapsed;
    }
}