using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.DefinitionExport;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using ZimLabs.CoreLib;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="Controls.DefinitionExportControlViewModel"/>
/// </summary>
internal class DefinitionExportControlViewModel : ViewModelBase, IConnection
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
    /// Backing field for <see cref="Objects"/>
    /// </summary>
    private ObservableCollection<ObjectDto> _objects = new();

    /// <summary>
    /// Gets or sets the list with the objects
    /// </summary>
    public ObservableCollection<ObjectDto> Objects
    {
        get => _objects;
        private set => SetField(ref _objects, value);
    }

    /// <summary>
    /// Backing field for <see cref="ObjectTypes"/>
    /// </summary>
    private ObservableCollection<string> _objectTypes = new();

    /// <summary>
    /// Gets or sets the list with the types
    /// </summary>
    public ObservableCollection<string> ObjectTypes
    {
        get => _objectTypes;
        private set => SetField(ref _objectTypes, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedObjectType"/>
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
            if (SetField(ref _selectedObjectType, value))
                FilterList();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ExportDir"/>
    /// </summary>
    private string _exportDir = string.Empty;

    /// <summary>
    /// Gets or sets the path of the export directory
    /// </summary>
    public string ExportDir
    {
        get => _exportDir;
        set => SetField(ref _exportDir, value);
    }

    /// <summary>
    /// Backing field for <see cref="InfoList"/>
    /// </summary>
    private string _infoList = string.Empty;

    /// <summary>
    /// Gets or sets the information of the export
    /// </summary>
    public string InfoList
    {
        get => _infoList;
        set => SetField(ref _infoList, value);
    }

    /// <summary>
    /// Backing field for <see cref="CreateTypeDir"/>
    /// </summary>
    private bool _createTypeDir;

    /// <summary>
    /// Gets or sets the value which indicates if a sub dir should be created for reach type
    /// </summary>
    public bool CreateTypeDir
    {
        get => _createTypeDir;
        set => SetField(ref _createTypeDir, value);
    }

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
            SetField(ref _filter, value);
            if (string.IsNullOrEmpty(value))
                FilterList();
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// The command to browse for the export directory
    /// </summary>
    public ICommand BrowseCommand => new DelegateCommand(BrowseExportDir);

    /// <summary>
    /// The command to load the data
    /// </summary>
    public ICommand ReloadCommand => new DelegateCommand(() =>
    {
        _dataLoaded = false;
        LoadData();
    });

    /// <summary>
    /// The command to set the selection
    /// </summary>
    public ICommand SelectCommand => new RelayCommand<SelectionType>(SetSelection);

    /// <summary>
    /// The command to export the selected entries
    /// </summary>
    public ICommand ExportCommand => new DelegateCommand(Export);

    /// <summary>
    /// The command to filter the list
    /// </summary>
    public ICommand FilterCommand => new DelegateCommand(FilterList);

    /// <summary>
    /// The command to clear the object list
    /// </summary>
    public ICommand ClearObjectListCommand => new DelegateCommand(() =>
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
        Objects = new ObservableCollection<ObjectDto>();
        _setText?.Invoke(string.Empty);
        InfoList = string.Empty;

        _manager?.Dispose();

        _manager = new DefinitionExportManager(_settingsManager!, dataSource, database);
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

        await ShowProgressAsync("Loading", "Please wait while loading the objects...");

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
            await CloseProgressAsync();
        }
    }

    /// <summary>
    /// Filters the list
    /// </summary>
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
    private async void Export()
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

        await ShowProgressAsync("Please wait", "Please wait while exporting the definitions...");

        try
        {
            _manager.Progress += (_, msg) =>
            {
                InfoList += $"{DateTime.Now:HH:mm:ss} | {msg}{Environment.NewLine}";
            };

            await _manager.ExportAsync(Objects.ToList(), objectList, ExportDir, CreateTypeDir);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            await CloseProgressAsync();
        }
    }

    /// <summary>
    /// Browse for the export directory
    /// </summary>
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
    private void SetSelection(SelectionType type)
    {
        foreach (var entry in Objects)
        {
            entry.Export = type == SelectionType.All;
        }
    }
}