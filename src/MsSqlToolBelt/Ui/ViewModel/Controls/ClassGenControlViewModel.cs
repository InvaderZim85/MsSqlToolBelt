﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.View.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.ClassGenControl"/>
/// </summary>
internal partial class ClassGenControlViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the class generator
    /// </summary>
    private ClassGenManager? _manager;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _settingsManager;

    /// <summary>
    /// Contains the value which indicates if the data already loaded
    /// </summary>
    private bool _dataLoaded;

    /// <summary>
    /// The action to set the SQL text
    /// </summary>
    private Action<ClassGenResult>? _setCode;

    /// <summary>
    /// The action to set the visibility of a column (true = visible, false = hidden)
    /// </summary>
    private Action<bool>? _setColumnVisibility;

    /// <summary>
    /// The action to open the selected entry in the search
    /// </summary>
    private Action<string>? _openInSearch;

    /// <summary>
    /// Contains the result of the class generator
    /// </summary>
    private ClassGenResult _classGenResult = new();

    /// <summary>
    /// Contains the value which indicates if the filter change should be ignored
    /// </summary>
    private bool _ignoreFilterChange;

    /// <summary>
    /// Gets or sets the preselection
    /// </summary>
    public string Preselection { get; set; } = string.Empty;

    #region View properties

    /// <summary>
    /// The list with the tables
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableDto> _tables = [];

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
            SetProperty(ref _selectedTable, value);

            if (_manager == null)
                return;

            _manager.SelectedTable = value;
            _classGenResult = new ClassGenResult();
            _setCode?.Invoke(_classGenResult);
            ButtonEfKeyCodeEnabled = false;
            ClassName = value?.Name ?? string.Empty;

            if (value is { Columns.Count: > 0 })
                SetColumns();
            else
                EnrichData();

            TableOptionEnabled = value is {Type: TableDtoType.Table};
            _setColumnVisibility?.Invoke(TableOptionEnabled);
        }
    }

    /// <summary>
    /// The list with the columns of the selected table
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColumnDto> _columns = [];

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

            // Remove the preselection if the filter was changed
            Preselection = string.Empty;

            if (_ignoreFilterChange)
                return;

            if (string.IsNullOrEmpty(value))
                FilterList();
        }
    }

    /// <summary>
    /// The list with the different types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IdTextEntry> _typeList = [];

    /// <summary>
    /// Backing field for <see cref="SelectedType"/>
    /// </summary>
    private IdTextEntry? _selectedType;

    /// <summary>
    /// Gets or sets the selected type
    /// </summary>
    public IdTextEntry? SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value) && value != null)
                FilterList();
        }
    }

    /// <summary>
    /// The header for the table list
    /// </summary>
    [ObservableProperty]
    private string _headerList = "Tables";

    /// <summary>
    /// The header for the column list
    /// </summary>
    [ObservableProperty]
    private string _headerColumns = "Columns";

    /// <summary>
    /// Gets or sets the visibility of the SQL Query region
    /// </summary>
    [ObservableProperty]
    private Visibility _sqlQueryVisibility = Visibility.Collapsed;

    /// <summary>
    /// Gets or sets the row height of the row which contains the SQL Query
    /// </summary>
    [ObservableProperty]
    private GridLength _sqlQueryRowHeight = GridLength.Auto;

    #region Options

    /// <summary>
    /// The list with the different modifiers
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _modifierList = [];

    /// <summary>
    /// The selected modifier
    /// </summary>
    [ObservableProperty]
    private string _selectedModifier = "public";

    /// <summary>
    /// The desired class name
    /// </summary>
    [ObservableProperty]
    private string _className = string.Empty;

    /// <summary>
    /// The value which indicates if the class should be "sealed"
    /// </summary>
    [ObservableProperty]
    private bool _optionSealedClass;

    /// <summary>
    /// The value which indicates if a EF Core class should be created
    /// </summary>
    [ObservableProperty]
    private bool _optionDbModel;

    /// <summary>
    /// Gets or sets the value which indicates if the column property should be added
    /// <para />
    /// Only for EF Core properties
    /// </summary>
    [ObservableProperty]
    private bool _optionColumnAttribute;

    /// <summary>
    /// The value which indicates if a backing field should be created
    /// </summary>
    [ObservableProperty]
    private bool _optionBackingField;

    /// <summary>
    /// The value which indicates if a summery should be created
    /// </summary>
    [ObservableProperty]
    private bool _optionSummary;

    /// <summary>
    /// The value which indicates if the "nullable" feature should be used
    /// </summary>
    [ObservableProperty]
    private bool _optionNullable;

    /// <summary>
    /// The value which indicates if the ef key code button is enabled
    /// </summary>
    [ObservableProperty]
    private bool _buttonEfKeyCodeEnabled;

    /// <summary>
    /// The value which indicates if the table options are enabled
    /// </summary>
    [ObservableProperty]
    private bool _tableOptionEnabled;

    /// <summary>
    /// The value which indicates if the table name should be added in the summary
    /// </summary>
    [ObservableProperty]
    private bool _optionAddTableNameInSummary;

    /// <summary>
    /// Backing field for <see cref="OptionSetField"/>
    /// </summary>
    [ObservableProperty]
    private bool _optionSetField;

    /// <summary>
    /// The desired namespace of the class
    /// </summary>
    [ObservableProperty]
    private string _namespace = string.Empty;

    /// <summary>
    /// Occurs when the user changes the db model option
    /// </summary>
    /// <param name="value">The new value</param>
    partial void OnOptionDbModelChanged(bool value)
    {
        // If the user uncheck the option, remove the "column attribute" option
        if (!value)
            OptionColumnAttribute = false;
    }

    /// <summary>
    /// Occurs when the user changes the column attribute option
    /// </summary>
    /// <param name="value">The new value</param>
    partial void OnOptionColumnAttributeChanged(bool value)
    {
        // If the user checks the option, set the db model option
        if (value)
            OptionDbModel = true;
    }

    /// <summary>
    /// Occurs when the user changes the backing field option
    /// </summary>
    /// <param name="value">The new value</param>
    partial void OnOptionBackingFieldChanged(bool value)
    {
        if (!value)
            OptionSetField = false;
    }

    /// <summary>
    /// Occurs when the user changes the option "set field"
    /// </summary>
    /// <param name="value">The new value</param>
    partial void OnOptionSetFieldChanged(bool value)
    {
        if (value)
            OptionBackingField = true;
    }

    #endregion

    #endregion

    #region Basics
    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="settingsManager">The instance for the interaction with the settings</param>
    /// <param name="setCode">The action to set the sql text</param>
    /// <param name="setColumnVisibility">The action to set the visibility of a column</param>
    /// <param name="openInSearch">The action to open the selected entry in the search</param>
    public void InitViewModel(SettingsManager settingsManager, Action<ClassGenResult> setCode, Action<bool> setColumnVisibility, Action<string> openInSearch)
    {
        _settingsManager = settingsManager;
        _setCode = setCode;
        _setColumnVisibility = setColumnVisibility;
        _openInSearch = openInSearch;

        var tmpList = new List<IdTextEntry>
        {
            new()
            {
                Id = 0,
                Text = "All"
            }
        };

        tmpList.AddRange(Helper.CreateTableDtoTypeList());
        TypeList = tmpList.ToObservableCollection();
        SelectedType = TypeList.FirstOrDefault(f => f.Id == 0);

        Mediator.AddAsyncFunction(MediatorKey.SetClassGenQueryVisibility, SetSqlQueryVisibilityAsync);
    }

    /// <summary>
    /// Sets the connection
    /// </summary>
    /// <param name="dataSource">The data source</param>
    /// <param name="database">The database</param>
    public void SetConnection(string dataSource, string database)
    {
        // Clear the current result
        Tables = [];
        Columns = [];
        _setCode?.Invoke(new ClassGenResult());

        _manager?.Dispose();
        _dataLoaded = false;

        _manager = new ClassGenManager(_settingsManager!, dataSource, database);
    }

    /// <summary>
    /// Closes the connection
    /// </summary>
    public void CloseConnection()
    {
        _manager?.Dispose();
    }
    #endregion

    #region Load / Show
    /// <summary>
    /// Loads the data
    /// </summary>
    /// <param name="showProgress"><see langword="true"/> to show the progress, <see langword="false"/> to hide the progress information (optional)</param>
    public async Task LoadDataAsync(bool showProgress = true)
    {
        if (_dataLoaded || _manager == null)
        {
            if (!string.IsNullOrEmpty(Preselection))
                FilterList();

            return;
        }

        ProgressDialogController? controller = null;
        if (showProgress && string.IsNullOrEmpty(Preselection))
            controller = await ShowProgressAsync("Loading", "Please wait while loading the tables...");

        try
        {
            ModifierList = CommonValues.GetModifierList();

            await _manager.LoadTablesAsync();

            _dataLoaded = true;

            FilterList();

            // Set the options
            await SetOptionsAsync();
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
    /// Loads and sets the class generator options
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task SetOptionsAsync()
    {
        if (_manager == null)
            return;

        // Load the options
        var options = await _manager.LoadClassGenOptionsAsync();

        // Set the options
        foreach (var (option, value) in options)
        {
            switch (option)
            {
                case ClassGenOption.AddColumnAttribute:
                    OptionColumnAttribute = value;
                    break;
                case ClassGenOption.AddSetField:
                    OptionSetField = value;
                    break;
                case ClassGenOption.AddSummary:
                    OptionSummary = value;
                    break;
                case ClassGenOption.AddTableNameInSummary:
                    OptionAddTableNameInSummary = value;
                    break;
                case ClassGenOption.DbModel:
                    OptionDbModel = value;
                    break;
                case ClassGenOption.Nullable:
                    OptionNullable = value;
                    break;
                case ClassGenOption.SealedClass:
                    OptionSealedClass = value;
                    break;
                case ClassGenOption.WithBackingField:
                    OptionBackingField = value;
                    break;
            }
        }

        // Load the desired modifier
        SelectedModifier = await SettingsManager.LoadSettingsValueAsync(SettingsKey.ClassGenDefaultModifier, ClassGenManager.ModifierFallback);

        // Set the "hide sql" option
        await SetSqlQueryVisibilityAsync();
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
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Filters the result
    /// </summary>
    [RelayCommand]
    private void FilterList()
    {
        if (_manager == null)
            return;

        // Check if there is a filter, when there is a filter and the filter doesn't contain the preselection
        // remove the filter, otherwise we can't show the desired table
        if (!string.IsNullOrEmpty(Preselection) && !string.IsNullOrEmpty(Filter) &&
            !Filter.Contains(Preselection, StringComparison.OrdinalIgnoreCase))
        {
            // Ignore the filter change, otherwise the filter change (= string.empty) will execute this method
            _ignoreFilterChange = true;
            Filter = string.Empty; // Remove the filter
        }

        var result = string.IsNullOrEmpty(Filter)
            ? _manager.Tables
            : _manager.Tables.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        if (SelectedType != null && SelectedType.Id != 0)
        {
            result = result.Where(w => (int) w.Type == SelectedType.Id).ToList();
        }

        Tables = result.ToObservableCollection();

        HeaderList = Tables.Count > 1 ? $"{Tables.Count} entries" : "1 entry";

        if (string.IsNullOrEmpty(Preselection))
            return;

        SelectedTable = Tables.FirstOrDefault(f => f.Name.Equals(Preselection));

        _ignoreFilterChange = false;
    }

    /// <summary>
    /// Sets the columns
    /// </summary>
    private void SetColumns()
    {
        if (_manager?.SelectedTable == null)
            return;

        Columns = _manager.SelectedTable.Columns.ToObservableCollection();

        HeaderColumns = Columns.Count > 1 ? $"{Columns.Count} columns" : "1 column";
    }
    #endregion

    #region Class gen
    /// <summary>
    /// Clears the aliases and removes the selection
    /// </summary>
    [RelayCommand]
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
    [RelayCommand]
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
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task GenerateClassAsync()
    {
        if (_manager?.SelectedTable == null)
            return;

        if (!ClassGenManager.ClassNameValid(ClassName))
        {
            await ShowMessageAsync(MessageHelper.ClassGenValidName);
            return;
        }

        // Check if there are any duplicates in the properties
        if (_manager.HasDuplicatedPropertyNames())
        {
            if (await ShowQuestionAsync(new MessageEntry("Duplicated properties",
                    "One or more names appear several times in the list of columns.",
                    "This can lead to errors in the generated class.",
                    "",
                    "Continue anyway?")) != MessageDialogResult.Affirmative)
                return;
        }

        var controller = await ShowProgressAsync("Generating", "Please wait while generating the class...");

        try
        {
            _classGenResult = await _manager.GenerateCodeAsync(GetOptions());

            _setCode?.Invoke(_classGenResult);

            ButtonEfKeyCodeEnabled = !string.IsNullOrEmpty(_classGenResult.EfCoreKeyCode);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Generate);
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
            DbModel = TableOptionEnabled && OptionDbModel,
            AddColumnAttribute = OptionColumnAttribute,
            WithBackingField = OptionBackingField,
            AddSummary = OptionSummary,
            Nullable = OptionNullable,
            AddSetField = OptionSetField,
            Namespace = Namespace,
            AddTableNameInSummary = OptionAddTableNameInSummary
        };
    }

    /// <summary>
    /// Generates a class from the inserted query
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task GenerateCodeFromQueryAsync()
    {
        if (_manager == null)
            return;

        if (!ClassGenManager.ClassNameValid(ClassName))
        {
            await ShowMessageAsync(MessageHelper.ClassGenValidName);
            return;
        }

        var dialog = new TextDialogWindow(new TextDialogSettings
        {
            Title = "SQL Query",
            Caption = "Insert the SQL query to generate a class from it. If the query does not have a where condition, one is automatically added to reduce the amount of data.",
            ShowOption = false,
            ShowValidateButton = true,
            ValidationFunc = DataHelper.ValidateSqlAsync,
            CodeType = CodeType.Sql
        })
        {
            Owner = GetMainWindow()
        };

        dialog.ShowDialog();

        if (string.IsNullOrEmpty(dialog.Code))
            return;

        var controller = await ShowProgressAsync("Generating", "Please wait while generating the class...");

        try
        {
            var options = GetOptions();
            // We ignore this, because it's not possible to gather "correct" table information
            // because a query can contain more than one table
            options.DbModel = false;
            options.SqlQuery = dialog.Code;
            _classGenResult = await _manager.GenerateCodeFromQueryAsync(options);

            _setCode?.Invoke(_classGenResult);

            ButtonEfKeyCodeEnabled = !string.IsNullOrEmpty(_classGenResult.EfCoreKeyCode);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Generate);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// The command to reload the content
    /// </summary>
    [RelayCommand]
    private async Task ReloadAsync()
    {
        _dataLoaded = false;
        await LoadDataAsync();
    }

    /// <summary>
    /// Opens the multi export window
    /// </summary>
    [RelayCommand]
    public void MultiExport()
    {
        if (_manager == null)
            return;

        var exportDialog = new ClassGenWindow(_manager) { Owner = GetMainWindow() };
        exportDialog.ShowDialog();
    }

    /// <summary>
    /// Opens the dialog window with the ef code for the primary key
    /// </summary>
    [RelayCommand]
    private void ShowEfKeyCode()
    {
        var dialog = new TextDialogWindow(new TextDialogSettings
        {
            Title = "Class generator - EF key code",
            Caption = "Code to configure multiple columns as key",
            CheckboxText = "Without method body",
            ShowOption = true,
            Text = _classGenResult.EfCoreKeyCode,
            TextOption = _classGenResult.EfCoreKeyCodeShort,
            CodeType = CodeType.CSharp
        })
        {
            Owner = GetMainWindow()
        };

        dialog.ShowDialog();
    }

    #endregion

    #region Various
    /// <summary>
    /// Clears the code
    /// </summary>
    /// <param name="type">The desired code type</param>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private void ClearCode(CodeType type)
    {
        _classGenResult = new ClassGenResult();

        switch (type)
        {
            case CodeType.CSharp:
                _classGenResult.ClassCode = string.Empty;
                break;
            case CodeType.Sql:
                _classGenResult.SqlQuery = string.Empty;
                break;
        }

        _setCode?.Invoke(_classGenResult);
    }

    /// <summary>
    /// Copies the code to the clipboard
    /// </summary>
    /// <param name="type">The desired code type</param>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private void CopyCode(CodeType type)
    {
        var content = type == CodeType.CSharp ? _classGenResult.ClassCode : _classGenResult.SqlQuery;
        if (string.IsNullOrEmpty(content))
            return;

        CopyToClipboard(content);
    }

    /// <summary>
    /// Clears everything
    /// </summary>
    public void Clear()
    {
        Tables = [];
        Columns = [];
        HeaderColumns = "Columns";
        HeaderList = "Tables";
        _classGenResult = new ClassGenResult();
        _setCode?.Invoke(_classGenResult);

        // Reset the options
        SelectedModifier = ModifierList.FirstOrDefault() ?? "public";
        ClassName = string.Empty;
        ButtonEfKeyCodeEnabled = false;
        ResetOptions();
    }

    /// <summary>
    /// Resets the options
    /// </summary>
    private void ResetOptions()
    {
        OptionSealedClass = false;
        OptionBackingField = false;
        OptionDbModel = false;
        OptionNullable = false;
        OptionSummary = false;
    }

    /// <summary>
    /// Shows the info of the set field option
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ShowSetFieldInfoAsync()
    {
        var result = await ShowQuestionAsync("Info",
            "The 'SetProperty' option uses a template that requires the 'ObservableObject' class which is a part of the \"CommunityToolkit.MVVM\" package (available via NuGet).",
            "Ok", "Show online");

        if (result != MessageDialogResult.Negative)
            return;

        const string gitHubLink = "https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/";

        Helper.OpenLink(gitHubLink);
    }

    /// <summary>
    /// Opens the selected table in the search
    /// </summary>
    [RelayCommand]
    private void OpenInSearch()
    {
        if (SelectedTable == null)
            return;

        _openInSearch?.Invoke(SelectedTable.Name);
    }

    /// <summary>
    /// Sets the visibility of the SQL query region
    /// </summary>
    private async Task SetSqlQueryVisibilityAsync()
    {
        var hideSqlQuery = await SettingsManager.LoadSettingsValueAsync(SettingsKey.ClassGenHideSqlQuery, false);

        if (hideSqlQuery)
        {
            SqlQueryVisibility = Visibility.Collapsed;
            SqlQueryRowHeight = GridLength.Auto;
        }
        else
        {
            SqlQueryVisibility = Visibility.Visible;
            SqlQueryRowHeight = new GridLength(1, GridUnitType.Star);
        }
    }
    #endregion
}