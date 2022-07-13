using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using ControlzEx.Theming;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using Serilog;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides several helper functions
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Init the logger
    /// </summary>
    public static void InitLogger()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.SQLite("logs/Log.db").CreateLogger();
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
            ? $"{value.TotalDays}d {value.Hours:00}:{value.Minutes:00}:{value.Seconds:00}"
            : $"{value.TotalHours:#00}:{value.Minutes:00}:{value.Seconds:00}";
    }

    /// <summary>
    /// Sets the color scheme
    /// </summary>
    /// <param name="colorScheme">The scheme which should be set</param>
    public static void SetColorTheme(string colorScheme)
    {
        ThemeManager.Current.ChangeThemeColorScheme(Application.Current, colorScheme);
    }

    /// <summary>
    /// Creates a list of the filter type enum
    /// </summary>
    /// <returns>The list with the entries</returns>
    public static List<IdTextEntry> CreateFilterTypeList()
    {
        return (from entry in Enum.GetValues<FilterType>()
            select new IdTextEntry
            {
                Id = (int) entry,
                Text = entry.GetAttribute<DescriptionAttribute>()?.Description ?? entry.ToString()
            }).ToList();
    }

    /// <summary>
    /// Creates the export list
    /// </summary>
    /// <param name="type">The type of the list</param>
    /// <returns>The list with the entries</returns>
    public static List<IdTextEntry> CreateExportTypeList(ExportDataType type)
    {
        var values = Enum.GetValues<ExportType>();

        var result = new List<IdTextEntry>();

        foreach (var value in values)
        {
            var attribute = value.GetAttribute<ExportTypeDescriptionAttribute>();
            if (attribute == null)
                continue;

            var entry = new IdTextEntry
            {
                Id = (int) value,
                Text = attribute.Description,
                BoundItem = attribute
            };

            switch (type)
            {
                case ExportDataType.List when attribute.List:
                    result.Add(entry);
                    break;
                case ExportDataType.Single when attribute.Single:
                    result.Add(entry);
                    break;
                default:
                    continue;
            }
        }

        return result;
    }

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
        return string.IsNullOrEmpty(value)
            ? value
            : $"{value[..1].ToLower()}{value[1..]}";
    }
}