using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.DefinitionExport;
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
        /// Contains the value which indicates if the data was already loaded
        /// </summary>
        private bool _dataLoaded;

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
                ExportButtonEnabled = !string.IsNullOrWhiteSpace(value) || EntryList != null && EntryList.Any();
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
        /// Contains the list with the original entries (needed for the filter)
        /// </summary>
        private List<DefinitionEntry> _originalList;

        /// <summary>
        /// Backing field for <see cref="EntryList"/>
        /// </summary>
        private ObservableCollection<DefinitionEntry> _entryList;

        /// <summary>
        /// Gets or sets the list with the available entries
        /// </summary>
        public ObservableCollection<DefinitionEntry> EntryList
        {
            get => _entryList;
            private set
            {
                SetField(ref _entryList, value);
                ExportButtonEnabled = !string.IsNullOrWhiteSpace(ObjectList) || value != null && value.Any();
            }
        }

        /// <summary>
        /// Backing field for <see cref="ListFilter"/>
        /// </summary>
        private string _listFilter;

        /// <summary>
        /// Gets or sets the filter string of the list
        /// </summary>
        public string ListFilter
        {
            get => _listFilter;
            set => SetField(ref _listFilter, value);
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
        /// The command to filter the list
        /// </summary>
        public ICommand FilterCommand => new DelegateCommand(FilterList);

        /// <summary>
        /// The command to set the export flag to true
        /// </summary>
        public ICommand SelectCommand => new DelegateCommand(() => SetSelection(true));

        /// <summary>
        /// The command to set the export flag to false
        /// </summary>
        public ICommand DeselectCommand => new DelegateCommand(() => SetSelection(false));

        /// <summary>
        /// The command to load the data
        /// </summary>
        public ICommand ReloadCommand => new DelegateCommand(LoadProcedures);

        /// <summary>
        /// Sets the connector
        /// </summary>
        /// <param name="connector">The connector</param>
        public void SetConnector(Connector connector)
        {
            _manager = new DefinitionExportManager(connector);
        }

        /// <summary>
        /// Loads the data
        /// </summary>
        public void LoadData()
        {
            if (_dataLoaded)
                return;

            LoadProcedures();

            _dataLoaded = true;
        }

        /// <summary>
        /// Loads the procedures and shows them
        /// </summary>
        private async void LoadProcedures()
        {
            var controller = await ShowProgress("Please wait", "Please wait while loading the procedures...");

            try
            {
                var result = await _manager.LoadProceduresAsync();
                if (!result.Any())
                    return;

                _originalList = result.OrderBy(o => o.Name).ToList();

                FilterList();
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Filters the list of entries according to the given filter
        /// </summary>
        private void FilterList()
        {
            if (_originalList == null)
            {
                EntryList = new ObservableCollection<DefinitionEntry>();
                return;
            }

            EntryList = new ObservableCollection<DefinitionEntry>(string.IsNullOrWhiteSpace(ListFilter)
                ? _originalList
                : _originalList.Where(w => w.Name.ContainsIgnoreCase(ListFilter)));
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

            if (string.IsNullOrWhiteSpace(ObjectList) && !EntryList.Any(a => a.Export))
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

                await _manager.ExportDefinitions(ExportDirectory, ObjectList, EntryList.ToList());
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
        /// Sets the "export" flag of every entry
        /// </summary>
        /// <param name="select">true to set export to true, otherwise false</param>
        private void SetSelection(bool select)
        {
            foreach (var entry in EntryList)
            {
                entry.Export = select;
            }
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            ExportDirectory = "";
            ObjectList = "";
            ExportInfo = "";

            // Reset the filter
            ListFilter = "";
            FilterList();
        }
    }
}
