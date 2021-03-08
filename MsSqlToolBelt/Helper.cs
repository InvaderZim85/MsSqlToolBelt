using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MsSqlToolBelt.DataObjects;
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
                .WriteTo.File("MsSqlToolBelt_Log.log",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level} | {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    shared: true)
                .CreateLogger();
        }

        /// <summary>
        /// Init the avalon editor
        /// </summary>
        /// <param name="editor">The desired editor</param>
        public static void InitAvalonEditor(TextEditor editor)
        {
            editor.Options.HighlightCurrentLine = true;
            editor.SyntaxHighlighting = LoadSqlSchema(true);
            editor.Foreground = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// Loads the highlight definition for the avalon editor
        /// </summary>
        /// <returns>The definition</returns>
        private static IHighlightingDefinition LoadSqlSchema(bool dark)
        {
            var fileName = dark ? "AvalonSqlSchema_Dark.xml" : "AvalonSqlSchema.xml";
            var file = Path.Combine(GetBaseFolder(), "SqlSchema", fileName);

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
        #endregion
    }
}