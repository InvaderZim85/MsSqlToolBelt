using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.CodeDom;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using ControlzEx.Theming;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Markdig;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.Types;
using Newtonsoft.Json;
using Serilog;

namespace MsSqlToolBelt
{
    /// <summary>
    /// Provides several helper functions
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Contains the path of the settings file
        /// </summary>
        private static readonly string SettingsFile = Path.Combine(GetBaseFolder(), "Settings.json");

        /// <summary>
        /// Contains the path of the file which contains the data types
        /// </summary>
        private static readonly string DataTypeFile = Path.Combine(GetBaseFolder(), "MsSqlToolBelt.DataTypes.json");

        /// <summary>
        /// Contains the list with the different types
        /// </summary>
        private static Dictionary<Type, string> _typeDict = new()
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            // Yes, this is an odd one.  Technically it's a type though.
            { typeof(void), "void" }
        };

        /// <summary>
        /// Backing field for <see cref="DataTypes"/>
        /// </summary>
        private static List<DataType> _dataTypes;

        /// <summary>
        /// Gets the list with the data types
        /// </summary>
        public static List<DataType> DataTypes
        {
            get
            {
                _dataTypes ??= LoadDataTypes();
                return _dataTypes;
            }
        }

        /// <summary>
        /// Init the logger
        /// </summary>
        public static void InitLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/MsSqlToolBelt_Log.log",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level} | {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .CreateLogger();
        }

        /// <summary>
        /// Sets the color theme of the application
        /// </summary>
        /// <param name="baseColor">The name of the base color (optional, if empty, the saved base color will be used)</param>
        /// <param name="colorTheme">The name of the color theme (optional, if empty, the saved color theme will be used)</param>
        public static void SetColorTheme(string baseColor = "", string colorTheme = "")
        {
            var settings = LoadSettings();
            if (string.IsNullOrEmpty(baseColor))
                baseColor = settings == null || string.IsNullOrEmpty(settings.ThemeBaseColor)
                    ? Properties.Settings.Default.BaseColor
                    : settings.ThemeBaseColor;

            if (string.IsNullOrEmpty(colorTheme))
                colorTheme = settings == null || string.IsNullOrEmpty(settings.ThemeColor)
                    ? Properties.Settings.Default.ColorTheme
                    : settings.ThemeColor;

            ThemeManager.Current.ChangeTheme(Application.Current, baseColor, colorTheme);

            ExecuteAction("SetTheme");
        }

        /// <summary>
        /// Init the avalon editor
        /// </summary>
        /// <param name="editor">The desired editor</param>
        /// <param name="type">The type of the schema</param>
        public static void InitAvalonEditor(TextEditor editor, CodeType type)
        {
            var dark = Properties.Settings.Default.BaseColor.Equals("Dark");
            editor.Options.HighlightCurrentLine = true;
            editor.SyntaxHighlighting = LoadSqlSchema(dark, type);
            editor.Foreground = new SolidColorBrush(dark ? Colors.White : Colors.Black);
        }

        /// <summary>
        /// Loads the highlight definition for the avalon editor
        /// </summary>
        /// <param name="dark">true to load the dark schema, otherwise false</param>
        /// <param name="type">The type of the schema</param>
        /// <returns>The definition</returns>
        private static IHighlightingDefinition LoadSqlSchema(bool dark, CodeType type)
        {
            var fileName = type == CodeType.CSharp
                ? dark
                    ? "AvalonCSharpSchema_Dark.xml"
                    : "AvalonCSharpSchema.xml"
                : dark
                    ? "AvalonSqlSchema_Dark.xml"
                    : "AvalonSqlSchema.xml";

            var file = Path.Combine(GetBaseFolder(), "Themes", fileName);

            using var reader = File.Open(file, FileMode.Open);
            using var xmlReader = new XmlTextReader(reader);

            return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
        }

        /// <summary>
        /// Loads the list off already used server
        /// </summary>
        /// <returns>The list with the server</returns>
        public static List<string> LoadServerList()
        {
            var settings = LoadSettings();

            return settings.ServerList;
        }

        /// <summary>
        /// Saves the new added server
        /// </summary>
        /// <param name="server">The name of the server</param>
        public static void SaveServer(string server)
        {
            var settings = LoadSettings();

            if (settings.ServerList.Any(a => a.ContainsIgnoreCase(server)))
                return;

            if (settings.ServerList.Count >= settings.ServerListCount)
                settings.ServerList.RemoveAt(0); // Remove the first element

            settings.ServerList.Add(server);

            SaveSettings(settings);
            Log.Information("Server '{server}' add.", server);
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        /// <returns>The settings</returns>
        public static Settings LoadSettings()
        {
            if (!File.Exists(SettingsFile))
                return new Settings();

            var content = File.ReadAllText(SettingsFile, Encoding.UTF8);

            return JsonConvert.DeserializeObject<Settings>(content);
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        /// <param name="data">The settings</param>
        /// <returns>true when successful, otherwise false</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool SaveSettings(Settings data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var content = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(SettingsFile, content, Encoding.UTF8);

            return File.Exists(SettingsFile);
        }

        /// <summary>
        /// Loads the list with the data types
        /// </summary>
        /// <returns>The list with the data types</returns>
        private static List<DataType> LoadDataTypes()
        {
            if (string.IsNullOrEmpty(DataTypeFile))
                return new List<DataType>();

            try
            {
                var content = File.ReadAllText(DataTypeFile, Encoding.UTF8);

                return JsonConvert.DeserializeObject<List<DataType>>(content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error has occurred in method '{method}'", nameof(LoadDataTypes));
                return new List<DataType>();
            }
        }

        /// <summary>
        /// Gets the path of the base folder
        /// </summary>
        /// <returns>The path of the base folder</returns>
        public static string GetBaseFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Gets the build date of the version
        /// </summary>
        /// <returns>The build date</returns>
        public static string GetBuildData()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            if (version == null)
                return "";

            var year = 2000 + version.Major;
            var buildDate = new DateTime(year, 1, 1);
            buildDate = buildDate.AddDays(version.Minor - 1);
            buildDate = buildDate.AddMinutes(version.Revision);
            var build = version.Build;

            return $"Build date: {buildDate:yyyy-MM-dd HH:mm}; build nr. {build + 1}";
        }

        /// <summary>
        /// Gets the full version name (application name, version number, build date)
        /// </summary>
        /// <returns>The full version name</returns>
        public static string GetFullVersionName()
        {
            var buildDate = GetBuildData();

            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            var name = assembly.GetName().Name;

            return $"{name} // Version: {version} // {buildDate}";
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
        /// Converts a markdown formatted content into a html page with the usage of <see cref="Markdown"/>
        /// </summary>
        /// <param name="markdown">The markdown formatted content</param>
        /// <returns>The html page</returns>
        public static string MarkdownToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return "";

            var dark = Properties.Settings.Default.BaseColor.Equals("Dark");
            var background = dark ? "#0D1117" : "#C9D1D9";
            var foreground = dark ? "#C9D1D9" : "#0D1117";

            var htmlContent = Markdown.ToHtml(markdown);

            var sb = new StringBuilder()
                .AppendLine("<html>")
                .AppendLine($"<body style='font-family:Segoe UI;background-color:{background};color:{foreground}'>")
                .Append(htmlContent)
                .AppendLine("</body>")
                .AppendLine("</html>");

            return sb.ToString();
        }

        #region Extensions
        /// <summary>
        /// Checks if the string value contains the given substring
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="substring">The sub string which should be found</param>
        /// <returns>true when the string value contains the substring, otherwise false</returns>
        public static bool ContainsIgnoreCase(this string value, string substring)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring))
                return false;

            return value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Check two strings for equality and ignores the casing
        /// </summary>
        /// <param name="value">The value which should be checked</param>
        /// <param name="match">The comparative value</param>
        /// <returns>true when the strings are equal, otherwise false</returns>
        public static bool EqualsIgnoreCase(this string value, string match)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(match))
                return false;

            return string.Equals(value, match, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts the first char of a string to upper case
        /// </summary>
        /// <param name="value">The original string</param>
        /// <param name="restToLower">true when the rest of the string should be converted to lower case, otherwise false (optional)</param>
        /// <returns>The converted string</returns>
        public static string FirstCharToUpper(this string value, bool restToLower = false)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : $"{value.Substring(0, 1).ToUpper()}{(restToLower ? value.Substring(1).ToLower() : value.Substring(1))}";
        }

        /// <summary>
        /// Converts the first char of a string to lower case
        /// </summary>
        /// <param name="value">The original value</param>
        /// <param name="restToLower">true when the rest of the string should be converted to lower case, otherwise false (optional)</param>
        /// <returns>The converted string</returns>
        public static string FirstCharToLower(this string value, bool restToLower = false)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : $"{value.Substring(0, 1).ToLower()}{(restToLower ? value.Substring(1).ToLower() : value.Substring(1))}";
        }

        /// <summary>
        /// Gets the attribute of the given object
        /// </summary>
        /// <typeparam name="T">The type of the attribute</typeparam>
        /// <param name="obj">The object</param> 
        /// <returns>The desired attribute of the object</returns>
        /// <exception cref="ArgumentNullException">Will be thrown when the object is null</exception>
        public static T GetAttribute<T>(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            // Get the attribute for classes
            var type = obj.GetType();

            var attribute = Attribute.GetCustomAttribute(type, typeof(T));

            if (attribute is T result)
                return result;

            return default;
        }

        /// <summary>
        /// Returns true if string is numeric and not empty or null or whitespace.
        /// Determines if string is numeric by parsing as Double
        /// </summary>
        /// <param name="value">The value which should be checked</param>
        /// <param name="style">Optional style - defaults to NumberStyles.Number (leading and trailing whitespace, leading and trailing sign, decimal point and thousands separator) </param>
        /// <param name="culture">Optional CultureInfo - defaults to InvariantCulture</param>
        /// <returns>true when the given string is a valid number, otherwise false</returns>
        public static bool IsNumeric(this string value, NumberStyles style = NumberStyles.Number,
            CultureInfo culture = null)
        {
            culture ??= CultureInfo.InvariantCulture;

            return double.TryParse(value, style, culture, out _) && !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Converts a string into a decimal ("1", "true" = true, rest false)
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="fallback">The fallback value which will used when the parse failed (optional).</param>
        /// <returns>The bool value</returns>
        public static bool ToBool(this string value, bool fallback = false)
        {
            if (string.IsNullOrEmpty(value))
                return fallback;

            if (value.Equals("1"))
                return true;

            return bool.TryParse(value, out var result) ? result : fallback;
        }

        /// <summary>
        /// Converts the specified size (bytes) into a readable format
        /// </summary>
        /// <param name="size">The size</param>
        /// <returns>The readable format</returns>
        public static string ConvertSize(this long size)
        {
            return size switch
            {
                < 1024 => $"{size} Bytes",
                >= 1024 when size < Math.Pow(1024, 2) => $"{size / 1024:N2} KB",
                _ when size >= Math.Pow(1024, 2) && size < Math.Pow(1024, 3) => $"{size / Math.Pow(1024, 2):N2} MB",
                _ when size >= Math.Pow(1024, 3) => $"{size / Math.Pow(1024, 3):N2} GB",
                _ => size.ToString()
            };
        }

        /// <summary>
        /// Gets the type of the specified value
        /// </summary>
        /// <param name="typeName">The type name</param>
        /// <returns>The C# alias. If the type is not known or the type is null, the original type name will be returned</returns>
        public static string GetTypeAlias(string typeName)
        {
            var type = Type.GetType(typeName);

            return type != null ? _typeDict.TryGetValue(type, out var value) ? value : type.Name : typeName;
        }
        #endregion

        #region Mediator

        /// <summary>
        /// The list with the actions which should be executed when the specified key is selected
        /// </summary>
        private static readonly SortedList<string, List<Action>> Actions = new();

        /// <summary>
        /// Adds an action
        /// </summary>
        /// <param name="key">The key of the action</param>
        /// <param name="action">The action which should be executed</param>
        public static void AddAction(string key, Action action)
        {
            if (Actions.ContainsKey(key))
            {
                Actions[key].Add(action);
            }
            else
            {
                Actions.Add(key, new List<Action>
                {
                    action
                });
            }
        }

        /// <summary>
        /// Executes the action which of the desired key
        /// </summary>
        /// <param name="key">The key of the action</param>
        public static void ExecuteAction(string key)
        {
            if (!Actions.ContainsKey(key))
                return;

            var actions = Actions[key];
            foreach (var action in actions)
            {
                // Execute the action
                action();
            }
        }

        /// <summary>
        /// Removes all actions with the specified key
        /// </summary>
        /// <param name="key">The key of the action</param>
        public static void RemoveAction(string key)
        {
            if (Actions.ContainsKey(key))
            {
                Actions.Remove(key);
            }
        }

        /// <summary>
        /// Removes all actions
        /// </summary>
        public static void RemoveAllActions()
        {
            Actions.Clear();
        }
        #endregion
    }
}