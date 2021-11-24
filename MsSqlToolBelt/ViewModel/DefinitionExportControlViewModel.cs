using System;
using System.IO;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.Business;
using ZimLabs.Database.MsSql;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.DefinitionExportControl"/>
    /// </summary>
    internal sealed class DefinitionExportControlViewModel : ViewModelBase
    {
        /// <summary>
        /// The instance for the interaction with the definition data
        /// </summary>
        private DefinitionExportManager _manager;

        /// <summary>
        /// Backing field for <see cref="ExportDirectory"/>
        /// </summary>
        private string _exportDirectory;

        /// <summary>
        /// Gets or sets the path of the export directory
        /// </summary>
        public string ExportDirectory
        {
            get => _exportDirectory;
            set => SetField(ref _exportDirectory, value);
        }

        /// <summary>
        /// Backing field for <see cref="ObjectList"/>
        /// </summary>
        private string _objectList;

        /// <summary>
        /// Gets or sets the object list (user input)
        /// </summary>
        public string ObjectList
        {
            get => _objectList;
            set
            {
                SetField(ref _objectList, value);
                ExportButtonEnabled = !string.IsNullOrWhiteSpace(value);
            }
        }

        /// <summary>
        /// Backing field for <see cref="ExportInfo"/>
        /// </summary>
        private string _exportInfo;

        /// <summary>
        /// Gets or sets the export info
        /// </summary>
        public string ExportInfo
        {
            get => _exportInfo;
            set => SetField(ref _exportInfo, value);
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
        /// The command to browse for the export directory
        /// </summary>
        public ICommand BrowseCommand => new DelegateCommand(() =>
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            ExportDirectory = dialog.FileName;
        });

        /// <summary>
        /// The command to execute the export
        /// </summary>
        public ICommand ExportCommand => new DelegateCommand(Export);

        /// <summary>
        /// The command to clear the info
        /// </summary>
        public ICommand ClearCommand => new DelegateCommand(ClearInfo);

        /// <summary>
        /// The command to clear all inputs
        /// </summary>
        public ICommand ClearAllCommand => new DelegateCommand(Clear);

        /// <summary>
        /// The command to copy the info
        /// </summary>
        public ICommand CopyCommand => new DelegateCommand(Copy);

        /// <summary>
        /// Sets the connector
        /// </summary>
        /// <param name="connector">The connector</param>
        public void SetConnector(Connector connector)
        {
            _manager = new DefinitionExportManager(connector);
        }

        /// <summary>
        /// Starts the export
        /// </summary>
        private async void Export()
        {
            if (string.IsNullOrWhiteSpace(ExportDirectory))
                return;

            if (!Directory.Exists(ExportDirectory))
            {
                await ShowMessage("Error", $"The specified export directory ({ExportDirectory}) doesn't exist.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ObjectList))
                return;

            var dialog = await ShowProgress("Please wait", "Please wait while exporting the definitions...");

            try
            {
                _manager.Progress += (_, msg) =>
                {
                    ExportInfo += $"{Environment.NewLine}{msg}";
                };

                _manager.ProgressShort += (_, msg) =>
                {
                    dialog.SetMessage(msg);
                };

                await _manager.ExportDefinitions(ExportDirectory, ObjectList);
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                await dialog.CloseAsync();
            }
        }

        /// <summary>
        /// Copies the export info to the clipboard
        /// </summary>
        private void Copy()
        {
            if (string.IsNullOrWhiteSpace(ExportInfo))
                return;

            CopyToClipboard(ExportInfo);
        }

        /// <summary>
        /// Clears the info
        /// </summary>
        private void ClearInfo()
        {
            ExportInfo = "";
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            ExportDirectory = "";
            ObjectList = "";
            ExportInfo = "";
        }
    }
}
