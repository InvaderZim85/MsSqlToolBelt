using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Converts the <see cref="IEnumerable{T}"/> into an <see cref="ObservableCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="source">The source list</param>
    /// <returns>The observable collection</returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    }
}