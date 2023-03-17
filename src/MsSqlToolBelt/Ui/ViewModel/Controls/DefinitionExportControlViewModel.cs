using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
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
    private bool _dataLoaded;

    /// <summary>
    /// The functions to get the input text
    /// </summary>
    private Func<string>? _getText;

    /// <summary>
    /// The action to set the input text
    /// </summary>
    private Action<string>? _setText;

    #region View properties

    /// <summary>
    /// The list with the objects
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ObjectDto> _objects = new();

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
                FilterList();
        }
    }

    /// <summary>
    /// The path of the export directory
    /// </summary>
    [ObservableProperty]
    private string _exportDir = string.Empty;

    /// <summary>
    /// The information of the export
    /// </summary>
    [ObservableProperty]
    private string _infoList = string.Empty;

    /// <summary>
    /// The value which indicates if a sub dir should be created for each type
    /// </summary>
    [ObservableProperty]
    private bool _createTypeDir;

    /// <summary>
    /// Backing field for <see cref="Filter"/>
    /// </summary>
    private string _filter = string.Empty;

    /// <summary>
    /// Gets or sets the filter
    /// </summary>
    public string Filter
    {
        get => _filter;
        set
        {
            SetProperty(ref _filter, value);
            if (string.IsNullOrEmpty(value))
                FilterList();
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// The command to load the data
    /// </summary>
    public ICommand ReloadCommand => new RelayCommand(() =>
    {
        _dataLoaded = false;
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
        Objects = new ObservableCollection<ObjectDto>();
        _setText?.Invoke(string.Empty);
        InfoList = string.Empty;

        // Reset the manager
        _manager?.Dispose();
        _manager = new DefinitionExportManager(_settingsManager!, dataSource, database);

        // Reset the "data loaded" flag
        _dataLoaded = false;
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        _manager?.Dispose();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public async void LoadData()
    {
        if (_dataLoaded || _manager == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the objects...");

        try
        {
            await _manager.LoadObjectsAsync();

            // Set the types
            ObjectTypes = _manager.Types.ToObservableCollection();
            SelectedObjectType = _manager.Types.FirstOrDefault() ?? "All";

            _dataLoaded = true;

            FilterList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Filters the list
    /// </summary>
    [RelayCommand]
    private void FilterList()
    {
        var tmpResult = string.IsNullOrEmpty(Filter)
            ? _manager!.Objects
            : _manager!.Objects.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        tmpResult = SelectedObjectType.Equals("All")
            ? tmpResult
            : tmpResult.Where(w => w.Type.Equals(SelectedObjectType)).ToList();

        Objects = tmpResult.OrderBy(o => o.Type).ThenBy(t => t.Name).ToObservableCollection();
    }

    /// <summary>
    /// Starts the export
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ExportDefinitionAsync()
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
            _manager.Progress += (_, msg) =>
            {
                InfoList += $"{DateTime.Now:HH:mm:ss} | {msg}{Environment.NewLine}";
                controller.SetMessage(msg);
            };

            await _manager.ExportAsync(Objects.ToList(), objectList, ExportDir, CreateTypeDir);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
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
        var dialog = new FolderBrowserDialog
        {
            Description = "Selected the export directory",
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        ExportDir = dialog.SelectedPath;
    }

    /// <summary>
    /// Sets the selection
    /// </summary>
    /// <param name="type">The desired selection type</param>
    [RelayCommand]
    private void SetSelection(SelectionType type)
    {
        foreach (var entry in Objects)
        {
            entry.Export = type == SelectionType.All;
        }
    }
}