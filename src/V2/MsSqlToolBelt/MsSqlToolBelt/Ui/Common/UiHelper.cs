using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MsSqlToolBelt.Common;
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

    /// <summary>
    /// Converts the list into a <see cref="ObservableCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="list">The list</param>
    /// <returns>The observable collection</returns>
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
    {
        return new ObservableCollection<T>(list);
    }

    /// <summary>
    /// Copies the selected values as CSV formatted string to the clipboard
    /// </summary>
    /// <typeparam name="T">The type of the entries</typeparam>
    /// <param name="dataGrid">The desired data grid</param>
    public static void CopyToClipboard<T>(this DataGrid dataGrid) where T : class
    {
        var items = new List<T>();

        foreach (var item in dataGrid.SelectedItems)
        {
            if (item is not T entry)
                continue;
            
            items.Add(entry);
        }

        items.CopyGridToClipboard();
    }

    /// <summary>
    /// Copies the selected values as CSV formatted string to the clipboard
    /// </summary>
    /// <typeparam name="TEntry">The type of the entries</typeparam>
    /// <typeparam name="TKey">The type of the sorting key</typeparam>
    /// <param name="dataGrid">The desired data grid</param>
    /// <param name="sortFunc">The sorting function</param>
    public static void CopyToClipboard<TEntry, TKey>(this DataGrid dataGrid, Func<TEntry, TKey> sortFunc) where TEntry : class
    {
        var items = new List<TEntry>();

        foreach (var item in dataGrid.SelectedItems)
        {
            if (item is not TEntry entry)
                continue;

            items.Add(entry);
        }
        items.OrderBy(sortFunc).ToList().CopyGridToClipboard();
    }
}