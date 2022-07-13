using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.View.Common;
using ZimLabs.CoreLib;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.ClassGenControl"/>
/// </summary>
internal class ClassGenControlViewModel : ViewModelBase, IConnection
{
    /// <summary>
    /// The instance for the interaction with the class generator
    /// </summary>
    private ClassGenManager? _manager;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _dataLoaded;

    /// <summary>
    /// The action to set the SQL text
    /// </summary>
    private Action<string>? _setCode;

    /// <summary>
    /// Contains the sql text
    /// </summary>
    private string _code = string.Empty;

    #region View properties

    /// <summary>
    /// Backing field for <see cref="Tables"/>
    /// </summary>
    private ObservableCollection<TableDto> _tables = new();

    /// <summary>
    /// Gets or sets the list with the tables
    /// </summary>
    public ObservableCollection<TableDto> Tables
    {
        get => _tables;
        set => SetField(ref _tables, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedTable"/>
    /// </summary>
    private TableDto? _selectedTable;

    /// <summary>
    /// Gets or sets the selected table
    /// </summary>
    public TableDto? SelectedTable
    {
        get => _selectedTable;
        set
        {
            SetField(ref _selectedTable, value);

            if (_manager == null)
                return;

            _manager.SelectedTable = value;

            if (value != null && value.Columns.Any())
                SetColumns();
            else
                EnrichData();
        }
    }

    /// <summary>
    /// Backing field for <see cref="Columns"/>
    /// </summary>
    private ObservableCollection<ColumnDto> _columns = new();

    /// <summary>
    /// Gets or sets the list with the columns
    /// </summary>
    public ObservableCollection<ColumnDto> Columns
    {
        get => _columns;
        set => SetField(ref _columns, value);
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
        set => SetField(ref _filter, value);
    }

    /// <summary>
    /// Backing field for <see cref="HeaderList"/>
    /// </summary>
    private string _headerList = "Tables";

    /// <summary>
    /// Gets or sets the header for the list
    /// </summary>
    public string HeaderList
    {
        get => _headerList;
        set => SetField(ref _headerList, value);
    }

    /// <summary>
    /// Backing field for <see cref="HeaderColumns"/>
    /// </summary>
    private string _headerColumns = "Columns";

    /// <summary>
    /// Gets or sets the column header
    /// </summary>
    public string HeaderColumns
    {
        get => _headerColumns;
        set => SetField(ref _headerColumns, value);
    }

    #region Options

    /// <summary>
    /// Backing field for <see cref="ModifierList"/>
    /// </summary>
    private ObservableCollection<string> _modifierList = new()
    {
        "public",
        "internal"
    };

    /// <summary>
    /// Gets or sets the list with the modifier
    /// </summary>
    public ObservableCollection<string> ModifierList
    {
        get => _modifierList;
        set => SetField(ref _modifierList, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedModifier"/>
    /// </summary>
    private string _selectedModifier = "public";

    /// <summary>
    /// Gets or sets the selected modifier
    /// </summary>
    public string SelectedModifier
    {
        get => _selectedModifier;
        set => SetField(ref _selectedModifier, value);
    }

    /// <summary>
    /// Backing field for <see cref="ClassName"/>
    /// </summary>
    private string _className = string.Empty;

    /// <summary>
    /// Gets or sets the name of the class
    /// </summary>
    public string ClassName
    {
        get => _className;
        set => SetField(ref _className, value);
    }

    /// <summary>
    /// Backing field for <see cref="OptionSealedClass"/>
    /// </summary>
    private bool _optionSealedClass;

    /// <summary>
    /// Gets or sets the value which indicates if a sealed class should be created
    /// </summary>
    public bool OptionSealedClass
    {
        get => _optionSealedClass;
        set => SetField(ref _optionSealedClass, value);
    }

    /// <summary>
    /// Backing field for <see cref="OptionDbModel"/>
    /// </summary>
    private bool _optionDbModel;

    /// <summary>
    /// Gets or sets the value which indicates if a DB model should be created
    /// </summary>
    public bool OptionDbModel
    {
        get => _optionDbModel;
        set => SetField(ref _optionDbModel, value);
    }

    /// <summary>
    /// Backing field for <see cref="OptionBackingField"/>
    /// </summary>
    private bool _optionBackingField;

    /// <summary>
    /// Gets or sets the value which indicates if a backing field should be created
    /// </summary>
    public bool OptionBackingField
    {
        get => _optionBackingField;
        set => SetField(ref _optionBackingField, value);
    }

    /// <summary>
    /// Backing field for <see cref="OptionSummary"/>
    /// </summary>
    private bool _optionSummary;

    /// <summary>
    /// Gets or sets the value which indicates if a summary should be added
    /// </summary>
    public bool OptionSummary
    {
        get => _optionSummary;
        set => SetField(ref _optionSummary, value);
    }

    /// <summary>
    /// Backing field for <see cref="OptionNullable"/>
    /// </summary>
    private bool _optionNullable;

    /// <summary>
    /// Gets or sets the value which indicates if the nullable property (.NET 6) should be used
    /// </summary>
    public bool OptionNullable
    {
        get => _optionNullable;
        set => SetField(ref _optionNullable, value);
    }
    #endregion

    #endregion

    /// <summary>
    /// The command to filter the table types
    /// </summary>
    public ICommand FilterCommand => new DelegateCommand(FilterResult);

    /// <summary>
    /// The command to reload the table types
    /// </summary>
    public ICommand ReloadCommand => new DelegateCommand(() =>
    {
        _dataLoaded = false;
        LoadData();
    });

    /// <summary>
    /// The command to select / unselect the columns
    /// </summary>
    public ICommand SelectCommand => new RelayCommand<SelectionType>(SelectColumns);

    /// <summary>
    /// The command to remove the alias from every entry
    /// </summary>
    public ICommand ClearAliasCommand => new DelegateCommand(ClearAlias);

    /// <summary>
    /// The command to clear the code
    /// </summary>
    public ICommand ClearCodeCommand => new DelegateCommand(ClearCode);

    /// <summary>
    /// The command to copy the code
    /// </summary>
    public ICommand CopyCodeCommand => new DelegateCommand(CopyCode);

    /// <summary>
    /// The command to generate the class
    /// </summary>
    public ICommand GenerateCommand => new DelegateCommand(GenerateClass);

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="setSqlText">The action to set the sql text</param>
    public void InitViewModel(Action<string> setSqlText)
    {
        _setCode = setSqlText;
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        // Clear the current result
        Tables = new ObservableCollection<TableDto>();
        Columns = new ObservableCollection<ColumnDto>();

        _manager?.Dispose();
        _dataLoaded = false;

        _manager = new ClassGenManager(dataSource, database);
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public async void LoadData()
    {
        if (_dataLoaded || _manager == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the tables...");

        try
        {
            await _manager.LoadTablesAsync();

            _dataLoaded = true;

            FilterResult();
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
    /// Enriches the selected table type
    /// </summary>
    private async void EnrichData()
    {
        if (_manager?.SelectedTable == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the columns...");

        try
        {
            await _manager.EnrichTableAsync();

            // Set the columns
            SetColumns();
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
    /// Filters the result
    /// </summary>
    private void FilterResult()
    {
        if (_manager == null)
            return;

        var result = string.IsNullOrEmpty(Filter)
            ? _manager.Tables
            : _manager.Tables.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        Tables = new ObservableCollection<TableDto>(result);
        HeaderList = Tables.Count > 1 ? $"{Tables.Count} tables" : "1 table";
    }

    /// <summary>
    /// Sets the columns
    /// </summary>
    private void SetColumns()
    {
        if (_manager?.SelectedTable == null)
            return;

        Columns = new ObservableCollection<ColumnDto>(_manager.SelectedTable.Columns);

        HeaderColumns = Columns.Count > 1 ? $"{Columns.Count} columns" : "1 column";
    }

    /// <summary>
    /// Clears the aliases and removes the selection
    /// </summary>
    private void ClearAlias()
    {
        if (!Columns.Any())
            return;

        foreach (var column in Columns)
        {
            column.Alias = string.Empty;
        }

        SelectColumns(SelectionType.None);
    }

    /// <summary>
    /// Selects / Deselects the columns
    /// </summary>
    /// <param name="type">The selection type</param>
    private void SelectColumns(SelectionType type)
    {
        if (!Columns.Any())
            return;

        foreach (var column in Columns)
        {
            column.Use = type == SelectionType.All;
        }
    }

    /// <summary>
    /// Generates the class
    /// </summary>
    private async void GenerateClass()
    {
        if (_manager?.SelectedTable == null)
            return;

        var controller = await ShowProgressAsync("Generating", "Please wait while generating the class...");

        try
        {
            var code = _manager.GenerateClassAsync(GetOptions());

            _setCode?.Invoke(code);
            _code = code;
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
    /// Gets the options
    /// </summary>
    /// <returns>The options</returns>
    private ClassGenOptions GetOptions()
    {
        return new ClassGenOptions
        {
            ClassName = ClassName,
            Modifier = SelectedModifier,
            SealedClass = OptionSealedClass,
            DbModel = OptionDbModel,
            WithBackingField = OptionBackingField,
            AddSummary = OptionSummary,
            Nullable = OptionNullable
        };
    }

    /// <summary>
    /// Clears the code
    /// </summary>
    private void ClearCode()
    {
        _code = string.Empty;
        _setCode?.Invoke(string.Empty);
    }

    /// <summary>
    /// Copies the code to the clipboard
    /// </summary>
    private void CopyCode()
    {
        if (string.IsNullOrEmpty(_code))
            return;

        CopyToClipboard(_code);
    }
}