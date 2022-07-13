using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MsSqlToolBelt.Common.Enums;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.Common;

/// <summary>
/// Provides several helper functions for the interaction with the UI
/// </summary>
internal static class UiHelper
{
    /// <summary>
    /// Init the avalon editor
    /// </summary>
    /// <param name="editor">The desired editor</param>
    /// <param name="type">The desired code type</param>
    public static void InitAvalonEditor(this TextEditor editor, CodeType type)
    {
        editor.Options.HighlightCurrentLine = true;
        var schema = LoadScheme(type);
        if (schema != null)
            editor.SyntaxHighlighting = schema;
        editor.Foreground = new SolidColorBrush(Colors.White);
    }

    /// <summary>
    /// Loads the scheme of the desired type
    /// </summary>
    /// <param name="type">The desired type</param>
    /// <returns>The definition</returns>
    private static IHighlightingDefinition? LoadScheme(CodeType type)
    {
        var files = Directory.GetFiles(Path.Combine(Core.GetBaseDirPath(), "Themes"));

        var file = files.FirstOrDefault(f => Path.GetFileName(f).ContainsIgnoreCase(type.ToString()) && f.ContainsIgnoreCase("Dark"));

        if (string.IsNullOrEmpty(file))
            return null;

        using var reader = File.Open(file, FileMode.Open);
        using var xmlReader = new XmlTextReader(reader);

        return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
    }
}