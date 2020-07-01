using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MsSqlToolBelt.DataObjects;
using Newtonsoft.Json;
using ZimLabs.CoreLib;
using ZimLabs.CoreLib.Extensions;
using Formatting = Newtonsoft.Json.Formatting;

namespace MsSqlToolBelt
{
    /// <summary>
    /// Provides several helper functions
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Contains the path of the file which contains the server
        /// </summary>
        private static readonly string ServerListFile = Path.Combine(Core.GetBaseFolder(), "ServerList.cfg");

        /// <summary>
        /// Contains the path of the settings file
        /// </summary>
        private static readonly string SettingsFile = Path.Combine(Core.GetBaseFolder(), "Settings.json");

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
        public static IHighlightingDefinition LoadSqlSchema(bool dark)
        {
            var fileName = dark ? "AvalonSqlSchema_Dark.xml" : "AvalonSqlSchema.xml";
            var file = Path.Combine(Core.GetBaseFolder(), "SqlSchema", fileName);

            using (var reader = File.Open(file, FileMode.Open))
            {
                using (var xmlReader = new XmlTextReader(reader))
                {
                    return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
                }
            }
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

            var content = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(SettingsFile, content, Encoding.UTF8);

            return File.Exists(SettingsFile);
        }
    }
}