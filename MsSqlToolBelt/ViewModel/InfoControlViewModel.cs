using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects;
using ZimLabs.TableCreator;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.InfoControl"/>
    /// </summary>
    internal sealed class InfoControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="LogDir"/>
        /// </summary>
        private string _logDir;

        /// <summary>
        /// Gets or sets the path of the log directory
        /// </summary>
        public string LogDir
        {
            get => _logDir;
            set => SetField(ref _logDir, value);
        }

        /// <summary>
        /// Backing field for <see cref="ReferenceList"/>
        /// </summary>
        private ObservableCollection<ReferenceEntry> _referenceList;

        /// <summary>
        /// Contains the list with the reference data
        /// </summary>
        public ObservableCollection<ReferenceEntry> ReferenceList
        {
            get => _referenceList;
            private set => SetField(ref _referenceList, value);
        }

        /// <summary>
        /// The command to export the data type list as a text file
        /// </summary>
        public ICommand ExportTextCommand => new DelegateCommand(() => ExportAs(OutputType.Default));

        /// <summary>
        /// The command to export the data type list as a CSV file
        /// </summary>
        public ICommand ExportCsvCommand => new DelegateCommand(() => ExportAs(OutputType.Csv));

        /// <summary>
        /// The command to export the data type list as a markdown file
        /// </summary>
        public ICommand ExportMdCommand => new DelegateCommand(() => ExportAs(OutputType.Markdown));

        /// <summary>
        /// The command to open the log directory
        /// </summary>
        public ICommand OpenLogDirCommand => new DelegateCommand(OpenLogDirectory);

        /// <summary>
        /// Loads and shows the data
        /// </summary>
        public async void InitViewModel()
        {
            try
            {
                var referenceList = PackageInfo.GetPackageInformation();

                ReferenceList = new ObservableCollection<ReferenceEntry>(referenceList);

                LogDir = Path.Combine(Helper.GetBaseFolder(), "logs");
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
        }

        /// <summary>
        /// Exports the data type list as a csv file
        /// </summary>
        /// <param name="outputType">The desired output type</param>
        private void ExportAs(OutputType outputType)
        {
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
                DefaultFileName = "NugetPackages",
                DefaultExtension = defaultExtension,
                Filters = { filter }
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            ReferenceList.SaveTable(dialog.FileName, Encoding.UTF8, outputType);
        }

        /// <summary>
        /// Navigates to the log directory in the explorer
        /// </summary>
        private void OpenLogDirectory()
        {
            if (string.IsNullOrEmpty(LogDir))
                return;

            Helper.ShowInExplorer(LogDir);
        }
    }
}
