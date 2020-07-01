using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.TableType;
using Newtonsoft.Json;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic of the export dialog
    /// </summary>
    internal sealed class ExportTableTypeViewModel : ViewModelBase
    {
        /// <summary>
        /// Contains the list with the table types
        /// </summary>
        private List<TableType> _tableTypes;

        /// <summary>
        /// Backing field for <see cref="InfoText"/>
        /// </summary>
        private string _infoText;

        /// <summary>
        /// Gets or sets the info text
        /// </summary>
        public string InfoText
        {
            get => _infoText;
            set => SetField(ref _infoText, value);
        }

        /// <summary>
        /// Backing field for <see cref="ExportTypeList"/>
        /// </summary>
        private ObservableCollection<TextValueItem> _exportTypeList;

        /// <summary>
        /// Gets or sets the list with the export types
        /// </summary>
        public ObservableCollection<TextValueItem> ExportTypeList
        {
            get => _exportTypeList;
            set => SetField(ref _exportTypeList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedExportType"/>
        /// </summary>
        private TextValueItem _selectedExportType;

        /// <summary>
        /// Gets or sets the selected export type
        /// </summary>
        public TextValueItem SelectedExportType
        {
            get => _selectedExportType;
            set => SetField(ref _selectedExportType, value);
        }

        /// <summary>
        /// Backing field for <see cref="DestinationPath"/>
        /// </summary>
        private string _destinationPath;

        /// <summary>
        /// Gets or sets the destination path
        /// </summary>
        public string DestinationPath
        {
            get => _destinationPath;
            set
            {
                SetField(ref _destinationPath, value);
                ExportButtonEnabled = GetSelectedExportType() != 0 &&
                                      !string.IsNullOrEmpty(value) &&
                                      Directory.Exists(Path.GetDirectoryName(value));
            }
        }

        /// <summary>
        /// Backing field for <see cref="ExportButtonEnabled"/>
        /// </summary>
        private bool _exportButtonEnabled;

        /// <summary>
        /// Gets or sets the value which indicates if the export button is enabled
        /// </summary>
        public bool ExportButtonEnabled
        {
            get => _exportButtonEnabled;
            set => SetField(ref _exportButtonEnabled, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="tableTypes">The list with the table types</param>
        public void InitViewModel(List<TableType> tableTypes)
        {
            _tableTypes = tableTypes;

            InfoText = $"{tableTypes.Count} table types";

            InitExportTypeList();
        }

        /// <summary>
        /// Init the export type list
        /// </summary>
        private void InitExportTypeList()
        {
            var tmpList = new List<TextValueItem>
            {
                new TextValueItem(1, "CSV"),
                new TextValueItem(2, "JSON"),
                new TextValueItem(3, "Markdown")
            };

            ExportTypeList = new ObservableCollection<TextValueItem>(tmpList);
            SelectedExportType = ExportTypeList.FirstOrDefault();
        }

        /// <summary>
        /// The command to browse for the destination path
        /// </summary>
        public ICommand BrowseCommand => new DelegateCommand(BrowseDestinationPath);

        /// <summary>
        /// The command to start the export
        /// </summary>
        public ICommand ExportCommand => new DelegateCommand(Export);

        /// <summary>
        /// Browse for the destination path
        /// </summary>
        private void BrowseDestinationPath()
        {
            var csvFilter = new CommonFileDialogFilter("CSV", "*.csv");
            var jsonFilter = new CommonFileDialogFilter("JSON", "*.json");
            var mdFilter = new CommonFileDialogFilter("Markdown", "*.md");

            var dialog = new CommonSaveFileDialog
            {
                Title = "Selected the desired path"
            };

            var id = GetSelectedExportType();
            switch (id)
            {
                case 0:
                case 1:
                    dialog.Filters.Add(csvFilter);
                    dialog.DefaultExtension = ".csv";
                    break;
                case 2:
                    dialog.Filters.Add(jsonFilter);
                    dialog.DefaultExtension = ".json";
                    break;
                case 3:
                    dialog.Filters.Add(mdFilter);
                    dialog.DefaultExtension = ".md";
                    break;
                default:
                    dialog.Filters.Add(csvFilter);
                    dialog.DefaultExtension = ".csv";
                    break;
            }

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            DestinationPath = dialog.FileName;
        }

        /// <summary>
        /// Exports the data
        /// </summary>
        private async void Export()
        {
            if (string.IsNullOrEmpty(DestinationPath) || GetSelectedExportType() == 0)
                return;

            var controller = await ShowProgress("Export", "Please wait while exporting the data...");

            try
            {
                Export(GetSelectedExportType());

                InfoText = "Data exported...";
            }
            catch (Exception ex)
            {
                await ShowMessage("Error",
                    $"An error has occured while exporting the data: {ex.Message}");
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Gets the id of the selected export type
        /// </summary>
        /// <returns>The id of the selected export type</returns>
        private int GetSelectedExportType()
        {
            return SelectedExportType?.Id ?? 0;
        }

        /// <summary>
        /// Exports the data according to the given type
        /// </summary>
        /// <param name="type">The export type</param>
        private void Export(int type)
        {
            switch (type)
            {
                case 1:
                    ExportCsv();
                    break;
                case 2:
                    ExportJson();
                    break;
                case 3:
                    ExportMarkdown();
                    break;
            }
        }

        /// <summary>
        /// Exports the data as CSV
        /// </summary>
        private void ExportCsv()
        {
            var result = new List<string>
            {
                "TableType;Column;DataType;Size;Nullable"
            };

            foreach (var type in _tableTypes)
            {
                result.AddRange(type.Columns.Select(column => $"{type.Name};{column.Name};{column.DataType};{column.Size};{column.Nullable}"));
            }

            File.WriteAllLines(DestinationPath, result, Encoding.UTF8);
        }

        /// <summary>
        /// Exports the data as JSOn
        /// </summary>
        private void ExportJson()
        {
            var data = JsonConvert.SerializeObject(_tableTypes, Formatting.Indented);

            File.WriteAllText(DestinationPath, data, Encoding.UTF8);
        }

        /// <summary>
        /// Exports the data as markdown
        /// </summary>
        private void ExportMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Table types");
            sb.AppendLine();

            sb.AppendLine("**Content**");

            foreach (var type in _tableTypes)
            {
                sb.AppendLine($"- [{type.Name}]({CreateLink(type.Name)})");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            foreach (var type in _tableTypes)
            {
                sb.AppendLine($"## {type.Name}");
                sb.AppendLine("| Column | Data type | Size | Null able |");
                sb.AppendLine("|--------|-----------|------|-----------|");

                foreach (var column in type.Columns)
                {
                    sb.AppendLine($"| {column.Name} | {column.DataType} | {column.Size} | {column.Nullable} |");
                }

                // Add one empty line add the end
                sb.AppendLine();
            }

            File.WriteAllText(DestinationPath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Creates the link
        /// </summary>
        /// <param name="name">The name of the type</param>
        /// <returns>The link</returns>
        private string CreateLink(string name)
        {
            // Step 1: All to lower
            name = name.ToLower();
            // Step 2: Remove all spaces
            name = name.Replace(" ", "-");

            return $"#{name}";
        }
    }
}
