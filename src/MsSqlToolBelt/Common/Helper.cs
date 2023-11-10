﻿using ControlzEx.Theming;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAPICodePack.Taskbar;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data.Internal;
using MsSqlToolBelt.DataObjects.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using ZimLabs.CoreLib;
using ZimLabs.Mapper;

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
        // Init the logger
        const string template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .WriteTo.SQLite("logs/Log.db")
            .WriteTo.File("logs/log_.log", rollingInterval: RollingInterval.Day, outputTemplate: template)
            .CreateLogger();

        // Init the taskbar
        if (TaskbarManager.IsPlatformSupported)
            _taskbarInstance = TaskbarManager.Instance;

        // Init the settings database
        using var context = new AppDbContext();
        var pendingMigrations = context.Database.GetPendingMigrations();
        context.Database.Migrate();
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
    /// Sets the color scheme
    /// </summary>
    /// <param name="colorScheme">The scheme which should be set</param>
    public static void SetColorTheme(string colorScheme)
    {
        ThemeManager.Current.ChangeThemeColorScheme(Application.Current, colorScheme);
    }

    /// <summary>
    /// Creates a list of the <see cref="FilterType"/> enum
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
    /// Creates a list of the <see cref="TableDtoType"/> enum
    /// </summary>
    /// <returns>The list with the entries</returns>
    public static List<IdTextEntry> CreateTableDtoTypeList()
    {
        return (from entry in Enum.GetValues<TableDtoType>()
            select new IdTextEntry
            {
                Id = (int) entry,
                Text = entry.GetAttribute<DescriptionAttribute>()?.Description ?? entry.ToString()
            }).ToList();
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
        var dayOfYear = version.Minor - 1; // we have to remove one day, because the date time starts always on the first day (we can't start at day 0)
        var build = version.Build + 1;
        var minutesSinceMidnight = version.Revision;

        var tmpDate = new DateTime(year, 1, 1).AddDays(dayOfYear).AddMinutes(minutesSinceMidnight);

        return (version.ToString(), $"{tmpDate:yyyy-MM-dd HH:mm} - build nr. {build}");
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

    /// <summary>
    /// Generates a hash value out of the different values
    /// </summary>
    /// <param name="values">The values</param>
    /// <returns>The generated hash code</returns>
    public static int GenerateHashCode(params string[] values)
    {
        if (values.Length == 0)
            return 0;

        var tmpValue = string.Join("", values);

        return tmpValue.GetHashCode();
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
}