using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using Newtonsoft.Json;
using Serilog;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides several functions for the export
/// </summary>
internal static class ExportHelper
{
    /// <summary>
    /// Converts the internal <see cref="ExportType"/> to the <see cref="OutputType"/> (needed for the export)
    /// </summary>
    /// <param name="type">The export type</param>
    /// <param name="fallback">The fallback (optional)</param>
    /// <returns>The output type</returns>
    public static OutputType ConvertToOutputType(ExportType type, OutputType fallback = OutputType.Default)
    {
        return type switch
        {
            ExportType.Ascii => OutputType.Default,
            ExportType.Csv => OutputType.Csv,
            ExportType.Markdown => OutputType.Markdown,
            _ => fallback
        };
    }

    /// <summary>
    /// Converts the internal <see cref="ExportType"/> to the <see cref="ListType"/> (needed for the export)
    /// </summary>
    /// <param name="type">The export type</param>
    /// <returns>The list type</returns>
    private static ListType ConvertToListType(ExportType type)
    {
        return type switch
        {
            ExportType.ListHyphens => ListType.Bullets,
            ExportType.ListNumbered => ListType.Numbers,
            _ => ListType.Bullets
        };
    }

    /// <summary>
    /// Creates the export / copy content of the object
    /// </summary>
    /// <param name="obj">The obj</param>
    /// <param name="type">The desired export type</param>
    /// <returns>The content</returns>
    public static string CreateObjectContent<T>(T obj, ExportType type) where T : class
    {
        if (type == ExportType.Json)
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        
        if (type is ExportType.Ascii or ExportType.Csv or ExportType.Markdown)
        {
            var outputType = ConvertToOutputType(type);
            return obj.CreateValueTable(outputType);
        }

        var listType = ConvertToListType(type);
        return obj.CreateValueList(listType);
    }

    /// <summary>
    /// Creates the export / copy content of the list
    /// </summary>
    /// <param name="list">The list</param>
    /// <param name="type">The desired export type</param>
    /// <returns>The content</returns>
    public static string CreateListContent(IEnumerable<object> list, ExportType type)
    {
        return list.CreateTable(ConvertToOutputType(type));
    }

    /// <summary>
    /// Exports a single object
    /// </summary>
    /// <param name="obj">The object which should be exported</param>
    /// <param name="filepath">The path of the file</param>
    /// <param name="type">The desired export type</param>
    /// <returns>The awaitable task</returns>
    public static async Task ExportObjectAsync<T>(T obj, string filepath, ExportType type) where T : class
    {
        if (type == ExportType.Json)
        {
            await ExportAsJsonAsync(obj, filepath);
            return;
        }

        var content = CreateObjectContent(obj, type);

        await File.WriteAllTextAsync(filepath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Exports a list of values
    /// </summary>
    /// <param name="list">The list which should be exported</param>
    /// <param name="filepath">The path of the file</param>
    /// <param name="type">The desired export type</param>
    /// <returns></returns>
    public static async Task ExportListAsync<T>(IEnumerable<T> list, string filepath, ExportType type) where T : class
    {
        if (type == ExportType.Json)
        {
            await ExportAsJsonAsync(list, filepath);
            return;
        }

        var content = CreateListContent(list, type);

        await File.WriteAllTextAsync(filepath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Exports the object (list or single) as JSON formatted string into the desired file
    /// </summary>
    /// <param name="value">The value which should be exported</param>
    /// <param name="filepath">The path of the file</param>
    /// <returns>The awaitable task</returns>
    private static async Task ExportAsJsonAsync(object value, string filepath)
    {
        var content = JsonConvert.SerializeObject(value, Formatting.Indented);

        await File.WriteAllTextAsync(filepath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Converts the data and copies them to the clipboard in the desired format
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="data">The data</param>
    public static async void CopyGridToClipboard<T>(this IReadOnlyCollection<T> data) where T : class
    {
        try
        {
            var settingsManager = new SettingsManager();
            var exportType =
                (ExportType)await settingsManager.LoadSettingsValueAsync(SettingsKey.CopyToClipboardFormat, DefaultEntries.CopyToClipboardFormat); // 1 = CSV

            var content = exportType switch
            {
                ExportType.Json => JsonConvert.SerializeObject(data, Formatting.Indented),
                _ => data.CreateTable(ConvertToOutputType(exportType))
            };

            Clipboard.SetText(content);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error has occurred while exporting the data.");
            Clipboard.Clear();
        }
    }
}