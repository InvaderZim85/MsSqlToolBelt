using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using ControlzEx.Theming;
using Markdig;
using Microsoft.WindowsAPICodePack.Taskbar;
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
    /// Contains the instance of the taskbar manager
    /// </summary>
    private static TaskbarManager? _taskbarInstance;

    /// <summary>
    /// Init the logger
    /// </summary>
    public static void InitHelper()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.SQLite("logs/Log.db").CreateLogger();
        if (TaskbarManager.IsPlatformSupported)
            _taskbarInstance = TaskbarManager.Instance;
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
    /// Gets the build information (Version / BuildDate)
    /// </summary>
    /// <returns>The build date of the program</returns>
    public static (string Version, string BuildDate) GetVersionInfo()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version == null)
            return (string.Empty, string.Empty);

        var year = version.Major + 2000; // add 2 thousand years because the year contains only the last two places but we need the complete year
        var dayOfYear = version.Minor;
        var build = version.Build + 1;
        var minutesSinceMidnight = version.Revision;

        var tmpDate = new DateTime(year, 1, 1).AddDays(dayOfYear).AddMinutes(minutesSinceMidnight);

        return (version.ToString(), $"Build date: {tmpDate:yyyy-MM-dd HH:mm} - build nr. {build}");
    }

    /// <summary>
    /// Opens the explorer and selects the specified file
    /// </summary>
    /// <param name="path">The path of the file</param>
    public static void ShowInExplorer(string path)
    {
        var arguments = Path.HasExtension(path) ? $"/n, /e, /select, \"{path}\"" : $"/n, /e, \"{path}\"";
        Process.Start("explorer.exe", arguments);
    }

    /// <summary>
    /// Opens the specified link
    /// </summary>
    /// <param name="url">The url of the link</param>
    public static void OpenLink(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }

    /// <summary>
    /// Converts a markdown formatted content into a html page with the usage of <see cref="Markdown"/>
    /// </summary>
    /// <param name="markdown">The markdown formatted content</param>
    /// <returns>The html page</returns>
    public static string MarkdownToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return "";

        const string background = "#252525";
        const string foreground = "#ffffff";

        var htmlContent = Markdown.ToHtml(markdown);

        var sb = new StringBuilder()
            .AppendLine("<html>")
            .AppendLine($"<body style='font-family:Segoe UI;background-color:{background};color:{foreground};font-size:12px'>")
            .Append(htmlContent)
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }

    #region Taskbar
    /// <summary>
    /// Sets the taskbar into an indeterminate state
    /// </summary>
    /// <param name="enable"><see langword="true"/> to set the indeterminate state, otherwise <see langword="false"/></param>
    public static void SetTaskbarIndeterminate(bool enable)
    {
        SetTaskbarState(enable, TaskbarProgressBarState.Indeterminate);
    }

    /// <summary>
    /// Sets the taskbar into an error state
    /// </summary>
    /// <param name="enable"><see langword="true"/> to set the indeterminate state, otherwise <see langword="false"/></param>
    public static void SetTaskbarError(bool enable)
    {
        SetTaskbarState(enable, TaskbarProgressBarState.Error);
    }

    /// <summary>
    /// Sets the taskbar into an pause state
    /// </summary>
    /// <param name="enable"><see langword="true"/> to set the indeterminate state, otherwise <see langword="false"/></param>
    public static void SetTaskbarPause(bool enable)
    {
        SetTaskbarState(enable, TaskbarProgressBarState.Paused);
    }

    /// <summary>
    /// Sets the taskbar state
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to set the state, <see langword="false"/> to set <see cref="TaskbarProgressBarState.NoProgress"/></param>
    /// <param name="state">The desired state</param>
    private static void SetTaskbarState(bool enabled, TaskbarProgressBarState state)
    {
        try
        {
            _taskbarInstance?.SetProgressState(enabled ? state : TaskbarProgressBarState.NoProgress);
            switch (enabled)
            {
                case true when state != TaskbarProgressBarState.Indeterminate:
                    _taskbarInstance?.SetProgressValue(100, 100);
                    break;
                case false:
                    _taskbarInstance?.SetProgressValue(0, 0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Can't change taskbar state");
        }
    }
    #endregion

    #region Extensions

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
            var size when size < divider => $"{size:N0} Bytes",
            var size when size >= divider && size < Math.Pow(divider, 2) => $"{size / divider:N2} KB",
            var size when size >= Math.Pow(divider, 2) && size < Math.Pow(divider, 3) =>
                $"{size / Math.Pow(divider, 2):N2} MB",
            var size when size >= Math.Pow(divider, 3) && size <= Math.Pow(divider, 4) => $"{size / Math.Pow(divider, 3):N2} GB",
            var size when size >= Math.Pow(divider, 4) => $"{size / Math.Pow(divider, 4)} TB",
            _ => value.ToString("N0")
        };
    }
    #endregion
}