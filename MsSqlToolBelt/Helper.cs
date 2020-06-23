using System.IO;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace MsSqlToolBelt
{
    /// <summary>
    /// Provides several helper functions
    /// </summary>
    internal static class Helper
    {
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
    }
}
