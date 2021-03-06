﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    internal sealed class SettingsWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="ServerListCount"/>
        /// </summary>
        private int _serverListCount;

        /// <summary>
        /// Gets or sets the amount of servers which should be saved
        /// </summary>
        public int ServerListCount
        {
            get => _serverListCount;
            set => SetField(ref _serverListCount, value);
        }

        /// <summary>
        /// Backing field for <see cref="ServerList"/>
        /// </summary>
        private ObservableCollection<string> _serverList;

        /// <summary>
        /// Gets or sets the list with the saved server
        /// </summary>
        public ObservableCollection<string> ServerList
        {
            get => _serverList;
            private set => SetField(ref _serverList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedServer"/>
        /// </summary>
        private string _selectedServer;

        /// <summary>
        /// Gets or sets the selected server
        /// </summary>
        public string SelectedServer
        {
            get => _selectedServer;
            set => SetField(ref _selectedServer, value);
        }

        /// <summary>
        /// Backing field for <see cref="TableIgnoreList"/>
        /// </summary>
        private ObservableCollection<TableIgnoreEntry> _tableIgnoreList;

        /// <summary>
        /// Gets or sets the list with the ignore entries
        /// </summary>
        public ObservableCollection<TableIgnoreEntry> TableIgnoreList
        {
            get => _tableIgnoreList;
            private set => SetField(ref _tableIgnoreList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedIgnoreEntry"/>
        /// </summary>
        private TableIgnoreEntry _selectedIgnoreEntry;

        /// <summary>
        /// Gets or sets the selected ignore entry
        /// </summary>
        public TableIgnoreEntry SelectedIgnoreEntry
        {
            get => _selectedIgnoreEntry;
            set => SetField(ref _selectedIgnoreEntry, value);
        }

        /// <summary>
        /// Backing field for <see cref="FilterList"/>
        /// </summary>
        private ObservableCollection<TextValueItem> _filterList;

        /// <summary>
        /// Gets or sets the list with the filter types
        /// </summary>
        public ObservableCollection<TextValueItem> FilterList
        {
            get => _filterList;
            private set => SetField(ref _filterList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedFilter"/>
        /// </summary>
        private TextValueItem _selectedFilter;

        /// <summary>
        /// Gets or sets the selected filter
        /// </summary>
        public TextValueItem SelectedFilter
        {
            get => _selectedFilter;
            set => SetField(ref _selectedFilter, value);
        }

        /// <summary>
        /// Backing field for <see cref="FilterValue"/>
        /// </summary>
        private string _filterValue;

        /// <summary>
        /// Gets or sets the filter value
        /// </summary>
        public string FilterValue
        {
            get => _filterValue;
            set => SetField(ref _filterValue, value);
        }

        /// <summary>
        /// Init the view model and loads the data
        /// </summary>
        public void InitViewModel()
        {
            SetFilterTypes();

            LoadSettings();
        }

        /// <summary>
        /// The command to save the settings
        /// </summary>
        public ICommand SaveSettingsCommand => new DelegateCommand(SaveSettings);

        /// <summary>
        /// The command to add a new server
        /// </summary>
        public ICommand AddServerCommand => new DelegateCommand(AddServer);

        /// <summary>
        /// The command to delete the selected server
        /// </summary>
        public ICommand DeleteServerCommand => new DelegateCommand(DeleteServer);

        /// <summary>
        /// The command to add a new filter
        /// </summary>
        public ICommand AddFilterCommand => new DelegateCommand(AddFilter);

        /// <summary>
        /// The command to delete the selected server
        /// </summary>
        public ICommand DeleteFilterCommand => new DelegateCommand(DeleteFilter);

        /// <summary>
        /// Prepares the filter types
        /// </summary>
        private void SetFilterTypes()
        {
            FilterList = new ObservableCollection<TextValueItem>(CustomEnums.GetFilterTypeList());
            SelectedFilter = FilterList.FirstOrDefault();
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        private async void LoadSettings()
        {
            var controller = await ShowProgress("Please wait", "Please wait while loading the settings...");

            try
            {
                var settings = Helper.LoadSettings();
                if (settings == null)
                {
                    ServerListCount = 10;
                    ServerList = new ObservableCollection<string>();
                    TableIgnoreList = new ObservableCollection<TableIgnoreEntry>();
                }
                else
                {
                    ServerListCount = settings.ServerListCount;
                    ServerList = new ObservableCollection<string>(settings.ServerList);
                    TableIgnoreList = new ObservableCollection<TableIgnoreEntry>(settings.TableIgnoreList);
                }
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
        /// Saves the settings
        /// </summary>
        private async void SaveSettings()
        {
            var controller = await ShowProgress("Please wait", "Please wait while saving the settings...");

            try
            {
                var settings = new Settings
                {
                    ServerListCount = ServerListCount,
                    ServerList = ServerList.ToList(),
                    TableIgnoreList = TableIgnoreList.ToList()
                };

                if (!Helper.SaveSettings(settings))
                    await ShowMessage("Error", "Can't save the settings.");
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
        /// Adds a new server to the list
        /// </summary>
        private async void AddServer()
        {
            var result = await ShowInput("Add server", "Add the path of the new server");

            if (string.IsNullOrEmpty(result))
                return;

            var existingEntry = ServerList.FirstOrDefault(f => f.EqualsIgnoreCase(result));
            if (!string.IsNullOrEmpty(existingEntry))
            {
                SelectedServer = existingEntry;
                return;
            }

            // Check if the server is "correct"
            var repo = new Repo(result);
            try
            {
                repo.LoadDatabases(); // Execute a test query...

                // Add the server to the list
                ServerList.Add(result);
                SelectedServer = ServerList.FirstOrDefault(f => f.Equals(result));
            }
            catch (Exception ex)
            {
                await ShowError(ex);
            }
            finally
            {
                repo.Dispose();
            }
        }

        /// <summary>
        /// Deletes the selected server
        /// </summary>
        private async void DeleteServer()
        {
            if (string.IsNullOrEmpty(SelectedServer))
                return;

            if (await ShowQuestion("Delete server", $"Do you really want to remove the server '{SelectedServer}'?") !=
                MessageDialogResult.Affirmative)
                return;

            ServerList.Remove(SelectedServer);
            SelectedServer = null;
        }

        /// <summary>
        /// Adds a new filter
        /// </summary>
        private void AddFilter()
        {
            if (SelectedFilter == null || string.IsNullOrEmpty(FilterValue))
                return;

            var newEntry = new TableIgnoreEntry
            {
                FilterType = (CustomEnums.FilterType) SelectedFilter.Id,
                Value = FilterValue
            };

            var existingEntry = TableIgnoreList.FirstOrDefault(f =>
                f.FilterType == newEntry.FilterType && f.Value.EqualsIgnoreCase(newEntry.Value));
            if (existingEntry != null)
            {
                SelectedIgnoreEntry = existingEntry;
                return;
            }

            TableIgnoreList.Add(newEntry);
            SelectedIgnoreEntry = newEntry;
        }

        /// <summary>
        /// Deletes the selected filter
        /// </summary>
        private async void DeleteFilter()
        {
            if (SelectedIgnoreEntry == null)
                return;

            if (await ShowQuestion("Remove filter",
                    $"Do you really want to remove the filter '{SelectedIgnoreEntry}'?") !=
                MessageDialogResult.Affirmative)
                return;

            TableIgnoreList.Remove(SelectedIgnoreEntry);
            SelectedIgnoreEntry = null;
        }
    }
}
