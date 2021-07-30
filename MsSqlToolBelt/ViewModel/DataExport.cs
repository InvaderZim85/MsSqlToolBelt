using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides functions to export a list of data
    /// </summary>
    internal static class DataExport
    {
        /// <summary>
        /// Exports the list as a ASCII-art, markdown table or a csv list
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="data">The list with the data</param>
        /// <param name="defaultFileName">The default file name</param>
        /// <param name="outputType">The desired output type</param>
        public static void Export<T>(this IEnumerable<T> data, string defaultFileName, OutputType outputType) where T : class
        {
            if (data == null)
            {
                Log.Warning("Data export failed because of missing data");
                return;
            }

            var filter = new CommonFileDialogFilter("Text file", "*.txt");
            var defaultExtension = "txt";
            var title = "Saves as ASCII styled table (text file)";

            switch (outputType)
            {
                case OutputType.Csv:
                    filter = new CommonFileDialogFilter("CSV file", "*.csv");
                    defaultExtension = "csv";
                    title = "Save as CSV file";
                    break;
                case OutputType.Markdown:
                    filter = new CommonFileDialogFilter("Markdown file", "*.md");
                    defaultExtension = "md";
                    title = "Save as markdown table";
                    break;
            }

            var dialog = new CommonSaveFileDialog
            {
                Title = title,
                DefaultFileName = defaultFileName,
                DefaultExtension = defaultExtension,
                Filters = { filter }
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            data.SaveTable(dialog.FileName, Encoding.UTF8, outputType);
        }
    }
}
