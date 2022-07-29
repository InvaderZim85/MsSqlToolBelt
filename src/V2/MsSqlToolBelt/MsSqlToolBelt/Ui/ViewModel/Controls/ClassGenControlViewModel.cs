﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.View.Windows;
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
    private Action<ClassGenResult>? _setCode;

    /// <summary>
    /// Contains the result of the class generator
    /// </summary>
    private ClassGenResult _classGenResult = new();

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
        private set => SetField(ref _tables, value);
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
            _classGenResult = new ClassGenResult();
            _setCode?.Invoke(_classGenResult);
            ButtonEfKeyCodeEnabled = false;

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
        private set => SetField(ref _columns, value);
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
        private set => SetField(ref _headerList, value);
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
        private set => SetField(ref _headerColumns, value);
    }

    #region Options

    /// <summary>
    /// Backing field for <see cref="ModifierList"/>
    /// </summary>
    private ObservableCollection<string> _modifierList = new()
    {
        "public",
        "internal",
        "protected",
        "protected internal"
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

    /// <summary>
    /// Backing field for <see cref="ButtonEfKeyCodeEnabled"/>
    /// </summary>
    private bool _buttonEfKeyCodeEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the ef key code button is enabled
    /// </summary>
    public bool ButtonEfKeyCodeEnabled
    {
        get => _buttonEfKeyCodeEnabled;
        set => SetField(ref _buttonEfKeyCodeEnabled, value);
    }
    #endregion

    #endregion

    #region Commands
    /// <summary>
    /// The command to filter the table types
    /// </summary>
    public ICommand FilterCommand => new DelegateCommand(FilterList);

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
    public ICommand ClearCodeCommand => new RelayCommand<CodeType>(ClearCode);

    /// <summary>
    /// The command to copy the code
    /// </summary>
    public ICommand CopyCodeCommand => new RelayCommand<CodeType>(CopyCode);

    /// <summary>
    /// The command to generate the class
    /// </summary>
    public ICommand GenerateCommand => new DelegateCommand(GenerateClass);

    /// <summary>
    /// The command to show the window with the data types
    /// </summary>
    public ICommand ShowTypeWindowCommand => new DelegateCommand(() =>
    {
        var dialog = new DataTypeWindow(_manager)
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    });

    /// <summary>
    /// The command to show the ef key code
    /// </summary>
    public ICommand ShowEfKeyCodeCommand => new DelegateCommand(() =>
    {
        var dialog = new TextDialogWindow(new TextDialogSettings
        {
            Title = "Class generator - EF",
            Caption = "Code to configure multiple columns as key",
            CheckboxText = "Without method body",
            ShowOption = true,
            Text = _classGenResult.EfCoreKeyCode,
            TextOption = _classGenResult.EfCoreKeyCodeShort,
            CodeType = CodeType.CSharp
        })
        {
            Owner = Application.Current.MainWindow
        };

        dialog.ShowDialog();
    });

    /// <summary>
    /// The command to generate a class from a query
    /// </summary>
    public ICommand FromQueryCommand => new DelegateCommand(GenerateCodeFromQuery);

    #endregion

    #region Basics
    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="setSqlText">The action to set the sql text</param>
    public void InitViewModel(Action<ClassGenResult> setSqlText)
    {
        _setCode = setSqlText;
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        // Clear the current result
        Tables = new ObservableCollection<TableDto>();
        Columns = new ObservableCollection<ColumnDto>();
        _setCode?.Invoke(new ClassGenResult());

        _manager?.Dispose();
        _dataLoaded = false;

        _manager = new ClassGenManager(dataSource, database);
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        _manager?.Dispose();
    }
    #endregion

    #region Load / Show
    /// <summary>
    /// Loads the data
    /// </summary>
    public async void LoadData()
    {
        if (_dataLoaded || _manager == null)
            return;

        await ShowProgressAsync("Loading", "Please wait while loading the tables...");

        try
        {
            await _manager.LoadTablesAsync();

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
    /// Enriches the selected table type
    /// </summary>
    private async void EnrichData()
    {
        if (_manager?.SelectedTable == null)
            return;

        await ShowProgressAsync("Loading", "Please wait while loading the columns...");

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
            await CloseProgressAsync();
        }
    }

    /// <summary>
    /// Filters the result
    /// </summary>
    private void FilterList()
    {
        if (_manager == null)
            return;

        var result = string.IsNullOrEmpty(Filter)
            ? _manager.Tables
            : _manager.Tables.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        Tables = result.ToObservableCollection();
        HeaderList = Tables.Count > 1 ? $"{Tables.Count} tables" : "1 table";
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

        if (!_manager.ClassNameValid(ClassName))
        {
            await ShowMessageAsync("Class generator",
                "Please enter a valid class name.\r\n\r\nHint: Must not start with a number and must not be empty");
            return;
        }

        await ShowProgressAsync("Generating", "Please wait while generating the class...");

        try
        {
            _classGenResult = _manager.GenerateCode(GetOptions());

            _setCode?.Invoke(_classGenResult);

            ButtonEfKeyCodeEnabled = !string.IsNullOrEmpty(_classGenResult.EfCoreKeyCode);
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
    /// Generates a class from the inserted query
    /// </summary>
    private async void GenerateCodeFromQuery()
    {
        if (_manager == null)
            return;

        if (!_manager.ClassNameValid(ClassName))
        {
            await ShowMessageAsync("Class generator",
                "Please enter a valid class name.\r\n\r\nHint: Must not start with a number and must not be empty");
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
            Owner = Application.Current.MainWindow
        };

        dialog.ShowDialog();

        if (string.IsNullOrEmpty(dialog.Code))
            return;

        await ShowProgressAsync("Please wait", "Please wait while generating the class...");

        try
        {
            // We ignore this, because it's not possible to gather "correct" table information
            // because a query can contain more than one table
            OptionDbModel = false;
            var options = GetOptions();
            options.SqlQuery = dialog.Code;
            _classGenResult = await _manager.GenerateFromQueryAsync(options);

            _setCode?.Invoke(_classGenResult);

            ButtonEfKeyCodeEnabled = !string.IsNullOrEmpty(_classGenResult.EfCoreKeyCode);
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
    /// Clears the code
    /// </summary>
    /// <param name="type">The desired code type</param>
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
        Tables = new ObservableCollection<TableDto>();
        Columns = new ObservableCollection<ColumnDto>();
        HeaderColumns = "Columns";
        HeaderList = "Tables";
        _classGenResult = new ClassGenResult();
        _setCode?.Invoke(_classGenResult);

        // Reset the options
        SelectedModifier = ModifierList.FirstOrDefault() ?? "public";
        ClassName = string.Empty;
        OptionSealedClass = false;
        OptionBackingField = false;
        OptionDbModel = false;
        OptionNullable = false;
        OptionSummary = false;
        ButtonEfKeyCodeEnabled = false;
    }
}