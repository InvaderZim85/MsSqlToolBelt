using ControlzEx.Theming;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAPICodePack.Taskbar;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data.Internal;
using MsSqlToolBelt.DataObjects.Common;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using MsSqlToolBelt.DataObjects.Internal;
using Newtonsoft.Json;
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
    /// Contains the list with the custom color schemes
    /// </summary>
    private static List<CustomColorScheme> _customColorSchemes = [];

    /// <summary>
    /// Init the logger
    /// </summary>
    public static void InitHelper()
    {
        // Init the logger
        const string template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "log_.log");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, outputTemplate: template)
            .CreateLogger();

        // Init the taskbar
        if (TaskbarManager.IsPlatformSupported)
            _taskbarInstance = TaskbarManager.Instance;

        // Init the settings database
        using var context = new AppDbContext();
        context.Database.Migrate();
    }

    #region Color theme

    /// <summary>
    /// Sets the color scheme
    /// </summary>
    /// <param name="colorScheme">The scheme which should be set</param>
    public static void SetColorTheme(string colorScheme)
    {
        var customColors = LoadCustomColors();
        if (customColors.Select(s => s.Name).Contains(colorScheme, StringComparer.OrdinalIgnoreCase))
        {
            var customColor = customColors.FirstOrDefault(f => f.Name.Equals(colorScheme, StringComparison.OrdinalIgnoreCase));
            if (customColor == null)
                return;
            
            var newTheme = new Theme("AppTheme", "AppTheme", "Dark", customColor.ColorValue.ToHex(), customColor.ColorValue,
                new SolidColorBrush(customColor.ColorValue), true, false);
            ThemeManager.Current.ChangeTheme(Application.Current, newTheme);
        }
        else
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, colorScheme);
    }

    /// <summary>
    /// Loads the custom colors
    /// </summary>
    /// <returns>The list with the custom color</returns>
    public static List<CustomColorScheme> LoadCustomColors()
    {
        if (_customColorSchemes.Count > 0)
            return _customColorSchemes;

        if (!File.Exists(DefaultEntries.CustomColorFile))
            return []; // Return an empty list

        try
        {
            var content = File.ReadAllText(DefaultEntries.CustomColorFile);

            _customColorSchemes = JsonConvert.DeserializeObject<List<CustomColorScheme>>(content) ?? [];

            return _customColorSchemes;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Can't load custom colors. File: '{path}'", DefaultEntries.CustomColorFile);
            return [];
        }
    }

    /// <summary>
    /// Saves the list with the custom colors
    /// </summary>
    /// <param name="customColors">The list with the custom colors</param>
    /// <returns><see langword="true"/> when the colors were saves successfully, otherwise <see langword="false"/></returns>
    public static bool SaveCustomColors(List<CustomColorScheme> customColors)
    {
        if (customColors.Count == 0)
            return true;

        try
        {
            var content = JsonConvert.SerializeObject(customColors, Formatting.Indented);

            File.WriteAllText(DefaultEntries.CustomColorFile, content);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Can't save custom colors. File: '{path}'", DefaultEntries.CustomColorFile);
            return false;
        }
    }

    /// <summary>
    /// Removes a custom color from the list
    /// </summary>
    /// <param name="name">The name of the color</param>
    /// <returns><see langword="true"/> when the color was deleted successfully, otherwise <see langword="false"/></returns>
    public static bool RemoveCustomColor(string name)
    {
        var customColors = LoadCustomColors();

        var customColor = customColors.FirstOrDefault(f => f.Name.Equals(name));
        if (customColor == null)
            return true;

        customColors.Remove(customColor);

        return SaveCustomColors(customColors);
    }
    #endregion

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