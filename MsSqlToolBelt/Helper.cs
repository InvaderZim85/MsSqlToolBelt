using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ZimLabs.CoreLib;
using ZimLabs.CoreLib.Extensions;

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
            var file = Path.Combine(ZimLabs.CoreLib.Core.GetBaseFolder(), "SqlSchema", fileName);

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
            return !File.Exists(ServerListFile)
                ? new List<string>()
                : File.ReadAllLines(ServerListFile, Encoding.UTF8).ToList();
        }

        /// <summary>
        /// Saves the new added server
        /// </summary>
        /// <param name="server">The name of the server</param>
        public static void SaveServer(string server)
        {
            var serverList = !File.Exists(ServerListFile) ? new List<string>() : File.ReadAllLines(ServerListFile).ToList();

            if (serverList.Any(a => a.ContainsIgnoreCase(server)))
                return;

            if (serverList.Count >= 10)
                serverList.RemoveAt(0); // Remove the first element

            serverList.Add(server);

            File.WriteAllLines(ServerListFile, serverList, Encoding.UTF8);
        }
    }
}