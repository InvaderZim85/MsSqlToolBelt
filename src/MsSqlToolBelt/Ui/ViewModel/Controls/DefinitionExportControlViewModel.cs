using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using MsSqlToolBelt.Ui.View.Common;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="Controls.DefinitionExportControlViewModel"/>
/// </summary>
internal partial class DefinitionExportControlViewModel : ViewModelBase, IConnection
{
    /// <summary>
    /// The instance for the interaction with the objects
    /// </summary>
    private DefinitionExportManager? _manager;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _settingsManager;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _objectDataLoaded;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _tableDataLoaded;

    /// <summary>
    /// The functions to get the input text
    /// </summary>
    private Func<string>? _getText;

    /// <summary>
    /// The action to set the input text
    /// </summary>
    private Action<string>? _setText;

    /// <summary>
    /// The action to set the controller message
    /// </summary>
    private Action<string>? _setControllerMessage;

    #region View properties

    /// <summary>
    /// Backing field <see cref="TabIndex"/>
    /// </summary>
    private int _tabIndex;

    /// <summary>
    /// Gets or sets the tab index
    /// </summary>
    public int TabIndex
    {
        get => _tabIndex;
        set
        {
            if (!SetProperty(ref _tabIndex, value)) 
                return;

            LoadData();
            OnPropertyChanged(nameof(ExportDir));
            OnPropertyChanged(nameof(InfoList));

            SubDirOptionEnabled = TabIndex == 0;
        }
    }

    /// <summary>
    /// The list with the objects
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DefinitionExportObject> _objects = new();

    /// <summary>
    /// The list with the types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _objectTypes = new();

    /// <summary>
    /// The selected type
    /// </summary>
    private string _selectedObjectType = string.Empty;

    /// <summary>
    /// Gets or sets the selected type
    /// </summary>
    public string SelectedObjectType
    {
        get => _selectedObjectType;
        set
        {
            if (SetProperty(ref _selectedObjectType, value))
                FilterObjectList();
        }
    }

    /// <summary>
    /// The list with the tables
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DefinitionExportObject> _tables = new();

    /// <summary>
    /// Backing field for <see cref="ExportDir"/>
    /// </summary>
    private string _objectExportDir = string.Empty;

    /// <summary>
    /// Backing field for <see cref="ExportDir"/>
    /// </summary>
    private string _tableExportDir = string.Empty;

    /// <summary>
    /// Gets or sets the export directory.
    /// <para />
    /// The path switches accordingly to the selected tab
    /// </summary>
    public string ExportDir
    {
        get => TabIndex switch
        {
            0 => _objectExportDir,
            1 => _tableExportDir,
            _ => string.Empty
        };
        set
        {
            switch (TabIndex)
            {
                case 0:
                    SetProperty(ref _objectExportDir, value);
                    break;
                case 1:
                    SetProperty(ref _tableExportDir, value);
                    break;
            }
        }
    }

    /// <summary>
    /// Backing field for <see cref="InfoList"/>
    /// </summary>
    private string _objectInfo = string.Empty;

    /// <summary>
    /// Backing field for <see cref="InfoList"/>
    /// </summary>
    private string _tableInfo = string.Empty;

    /// <summary>
    /// Gets or sets the export info
    /// <para />
    /// The info switches accordingly to the selected tab
    /// </summary>
    public string InfoList
    {
        get => TabIndex switch
        {
            0 => _objectInfo,
            1 => _tableInfo,
            _ => string.Empty
        };
        set
        {
            switch (TabIndex)
            {
                case 0:
                    SetProperty(ref _objectInfo, value);
                    break;
                case 1:
                    SetProperty(ref _tableInfo, value);
                    break;
            }
        }
    }

    /// <summary>
    /// The value which indicates if a sub dir should be created for each type
    /// <para />
    /// This value is only relevant for the object definition export
    /// </summary>
    [ObservableProperty]
    private bool _createTypeDir;

    /// <summary>
    /// Backing field for <see cref="FilterObject"/>
    /// </summary>
    private string _filterObject = string.Empty;

    /// <summary>
    /// Gets or sets the object filter
    /// </summary>
    public string FilterObject
    {
        get => _filterObject;
        set
        {
            SetProperty(ref _filterObject, value);
            if (string.IsNullOrEmpty(value))
                FilterObjectList();
        }
    }

    /// <summary>
    /// Backing field for <see cref="FilterTable"/>
    /// </summary>
    private string _filterTable = string.Empty;

    /// <summary>
    /// Gets or sets the table filter
    /// </summary>
    public string FilterTable
    {
        get => _filterTable;
        set
        {
            SetProperty(ref _filterTable, value);
            if (string.IsNullOrEmpty(value))
                FilterTableList();
        }
    }

    /// <summary>
    /// The value which indicates whether the sub dir option is enabled or not
    /// </summary>
    [ObservableProperty]
    private bool _subDirOptionEnabled = true;

    #endregion

    #region Commands

    /// <summary>
    /// The command to reload the objects
    /// </summary>
    public ICommand ReloadObjectsCommand => new RelayCommand(() =>
    {
        _objectDataLoaded = false;
        LoadData();
    });

    /// <summary>
    /// The command to reload the tables
    /// </summary>
    public ICommand ReloadTablesCommand => new RelayCommand(() =>
    {
        _tableDataLoaded = false;
        LoadData();
    });

    /// <summary>
    /// The command to clear the object list
    /// </summary>
    public ICommand ClearObjectListCommand => new RelayCommand(() =>
    {
        _setText?.Invoke(string.Empty);
    });
    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="settingsManager">The instance for the interaction with the settings</param>
    /// <param name="getText">The function to get the input text</param>
    /// <param name="setText">The action to set the input text</param>
    public void InitViewModel(SettingsManager settingsManager, Func<string> getText, Action<string> setText)
    {
        _settingsManager = settingsManager;
        _getText = getText;
        _setText = setText;
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        // Reset the lists
        Objects = new ObservableCollection<DefinitionExportObject>();
        _setText?.Invoke(string.Empty);
        InfoList = string.Empty;

        // Remove the event if it's already attached to prevent multiple events
        if (_manager != null)
            _manager.Progress -= ManagerProgressEvent;

        // Reset the manager
        _manager?.Dispose();
        _manager = new DefinitionExportManager(_settingsManager!, dataSource, database);

        // Add the event
        _manager.Progress += ManagerProgressEvent;

        // Reset the "data loaded" flag
        _objectDataLoaded = false;
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        _manager?.Dispose();
    }

    /// <summary>
    /// The progress event
    /// </summary>
    /// <param name="sender">The sender</param>
    /// <param name="message">The message</param>
    private void ManagerProgressEvent(object? sender, string message)
    {
        InfoList += $"{DateTime.Now:HH:mm:ss} | {message}{Environment.NewLine}";

        _setControllerMessage?.Invoke(message);
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    /// <param name="showProgress"><see langword="true"/> to show the progress, <see langword="false"/> to hide the progress information (optional)</param>
    public async void LoadData(bool showProgress = true)
    {
        if (_manager == null)
            return;

        ProgressDialogController? controller = null;
        
        if (showProgress)
            controller = await ShowProgressAsync("Loading", "Please wait while loading the objects...");

        try
        {
            switch (TabIndex)
            {
                case 0:
                    await LoadObjectDataAsync();
                    break;
                case 1:
                    await LoadTableDataAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            if (controller != null)
                await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Loads the object data
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task LoadObjectDataAsync()
    {
        if (_objectDataLoaded)
            return;

        await _manager!.LoadObjectsAsync();

        // Set the types
        ObjectTypes = _manager.Types.ToObservableCollection();
        SelectedObjectType = _manager.Types.FirstOrDefault() ?? "All";

        _objectDataLoaded = true;

        FilterObjectList();
    }

    /// <summary>
    /// Filters the list
    /// </summary>
    [RelayCommand]
    private void FilterObjectList()
    {
        var tmpResult = string.IsNullOrEmpty(FilterObject)
            ? _manager!.Objects
            : _manager!.Objects.Where(w => w.Name.ContainsIgnoreCase(FilterObject)).ToList();

        tmpResult = SelectedObjectType.Equals("All")
            ? tmpResult
            : tmpResult.Where(w => w.Type.Equals(SelectedObjectType)).ToList();

        Objects = tmpResult.OrderBy(o => o.Type).ThenBy(t => t.Name).ToObservableCollection();
    }

    /// <summary>
    /// Loads the table data
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task LoadTableDataAsync()
    {
        if (_tableDataLoaded)
            return;

        await _manager!.LoadTablesAsync();

        _tableDataLoaded = true;

        FilterTableList();
    }

    /// <summary>
    /// Filters the table list
    /// </summary>
    [RelayCommand]
    private void FilterTableList()
    {
        var tmpResult = string.IsNullOrEmpty(FilterTable)
            ? _manager!.Tables
            : _manager!.Tables.Where(w => w.Name.ContainsIgnoreCase(FilterTable)).ToList();

        Tables = tmpResult.ToObservableCollection();
    }

    /// <summary>
    /// Starts the export
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExportObjectDefinitionAsync()
    {
        if (_manager == null)
            return;

        InfoList = string.Empty;

        if (string.IsNullOrEmpty(ExportDir))
        {
            InfoList = "Please specify the export directory.";
            return;
        }

        if (!Directory.Exists(ExportDir))
        {
            InfoList = "The specified export directory doesn't exist.";
            return;
        }

        var objectList = _getText?.Invoke() ?? string.Empty;
        if (!Objects.Any(a => a.Export) && string.IsNullOrEmpty(objectList))
        {
            InfoList = "Select or add at least one entry for the export.";
            return;
        }

        var controller = await ShowProgressAsync("Please wait", "Please wait while exporting the definitions...");

        try
        {
            _setControllerMessage = controller.SetMessage;

            await _manager.ExportObjectsAsync(Objects.ToList(), objectList, ExportDir, CreateTypeDir);

            _setControllerMessage = null;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Starts the table export
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExportTableDefinitionAsync()
    {
        if (_manager == null)
            return;

        InfoList = string.Empty;

        if (string.IsNullOrEmpty(ExportDir))
        {
            InfoList = "Please specify the export directory.";
            return;
        }

        if (!Directory.Exists(ExportDir))
        {
            InfoList = "The specified export directory doesn't exist.";
            return;
        }

        if (Tables.All(a => !a.Export))
        {
            InfoList = "Select at least one entry for the export.";
            return;
        }

        var cts = new CancellationTokenSource();
        var controller = await ShowProgressAsync("Please wait", "Please wait while exporting the definitions...", cts);

        try
        {
            _setControllerMessage = controller.SetMessage;

            await _manager.ExportTablesAsync(Tables.ToList(), ExportDir, cts.Token);

            _setControllerMessage = null;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Browse for the export directory
    /// </summary>
    [RelayCommand]
    private void BrowseExportDir()
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Title = TabIndex switch
            {
                0 => "Selected the export directory for the objects",
                1 => "Selected the export directory for the tables",
                _ => "Selected the export directory"
            }
        };

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            return;

        ExportDir = dialog.FileName;
    }

    /// <summary>
    /// Sets the selection of the objects
    /// </summary>
    /// <param name="type">The desired selection type</param>
    [RelayCommand]
    private void SetObjectSelection(SelectionType type)
    {
        foreach (var entry in Objects)
        {
            entry.Export = type == SelectionType.All;
        }
    }

    /// <summary>
    /// Sets the selection of the tables
    /// </summary>
    /// <param name="type">The desired selection type</param>
    [RelayCommand]
    private void SetTableSelection(SelectionType type)
    {
        foreach (var table in Tables)
        {
            table.Export = type == SelectionType.All;
        }
    }
}