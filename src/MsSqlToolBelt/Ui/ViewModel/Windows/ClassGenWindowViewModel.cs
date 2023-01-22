using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Windows;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the functions for <see cref="ClassGenWindow"/>
/// </summary>
internal class ClassGenWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the class generator
    /// </summary>
    private ClassGenManager? _manager;

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
        private set => SetProperty(ref _tables, value);
    }

    /// <summary>
    /// Backing field for <see cref="InfoList"/>
    /// </summary>
    private string _infoList = string.Empty;

    /// <summary>
    /// Gets or sets the info
    /// </summary>
    public string InfoList
    {
        get => _infoList;
        set => SetProperty(ref _infoList, value);
    }
    
    /// <summary>
    /// Backing field for <see cref="ModifierList"/>
    /// </summary>
    private ObservableCollection<string> _modifierList = new();

    /// <summary>
    /// Gets or sets the list with the modifier
    /// </summary>
    public ObservableCollection<string> ModifierList
    {
        get => _modifierList;
        private set => SetProperty(ref _modifierList, value);
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
        set => SetProperty(ref _selectedModifier, value);
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
        set => SetProperty(ref _optionSealedClass, value);
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
        set => SetProperty(ref _optionDbModel, value);
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
        set
        {
            if (SetProperty(ref _optionBackingField, value) && !value)
                OptionSetField = false;
        }
    }

    /// <summary>
    /// Backing field for <see cref="OptionSetField"/>
    /// </summary>
    private bool _optionSetField;

    /// <summary>
    /// Gets or sets the value which indicates if the set field method should be used.
    /// <para />
    /// If this option is enabled, the option <see cref="OptionBackingField"/> will be enabled)
    /// </summary>
    public bool OptionSetField
    {
        get => _optionSetField;
        set
        {
            if (SetProperty(ref _optionSetField, value) && value)
                OptionBackingField = true;
        }
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
        set => SetProperty(ref _optionSummary, value);
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
        set => SetProperty(ref _optionNullable, value);
    }

    /// <summary>
    /// Backing field for <see cref="Namespace"/>
    /// </summary>
    private string _namespace = string.Empty;

    /// <summary>
    /// Gets or sets the desired namespace
    /// </summary>
    public string Namespace
    {
        get => _namespace;
        set => SetProperty(ref _namespace, value);
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
        set => SetProperty(ref _exportDir, value);
    }

    /// <summary>
    /// Backing field for <see cref="EmptyDirBeforeExport"/>
    /// </summary>
    private bool _emptyDirBeforeExport;

    /// <summary>
    /// Gets or sets the value which indicates if the export directory should be cleared before export
    /// </summary>
    public bool EmptyDirBeforeExport
    {
        get => _emptyDirBeforeExport;
        set => SetProperty(ref _emptyDirBeforeExport, value);
    }

    /// <summary>
    /// Backing field for <see cref="HeaderTables"/>
    /// </summary>
    private string _headerTables = "Tables";

    /// <summary>
    /// Gets or sets the header for the list
    /// </summary>
    public string HeaderTables
    {
        get => _headerTables;
        private set => SetProperty(ref _headerTables, value);
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
            SetProperty(ref _filter, value);
            if (string.IsNullOrEmpty(value))
                FilterList();
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// The command to browse for the export directory
    /// </summary>
    public ICommand BrowseCommand => new RelayCommand(BrowseExportDir);

    /// <summary>
    /// The command to remove the alias from every entry
    /// </summary>
    public ICommand ClearAliasCommand => new RelayCommand(ClearAlias);

    /// <summary>
    /// The command to select / unselect the columns
    /// </summary>
    public ICommand SelectCommand => new RelayCommand<SelectionType>(SelectTables);

    /// <summary>
    /// The command which occurs when the user hits the show info menu item (context menu of the set field option)
    /// </summary>
    public ICommand ShowSetFieldInfoCommand => new RelayCommand(ShowSetFieldInfo);

    /// <summary>
    /// The command which occurs when the user hits the generate button
    /// </summary>
    public ICommand GenerateClassesCommand => new RelayCommand(GenerateClasses);

    /// <summary>
    /// The command to filter the list
    /// </summary>
    public ICommand FilterCommand => new RelayCommand(FilterList);

    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="classGenManager">The instance of the class generator</param>
    public void InitViewModel(ClassGenManager classGenManager)
    {
        _manager = classGenManager;

        FilterList();

        ModifierList = _manager.GetModifierList();
        SelectedModifier = "public";

        // Add the info event
        _manager.Progress += (_, msg) =>
        {
            InfoList += $"{DateTime.Now:HH:mm:ss} | {msg}{Environment.NewLine}";
            SetProgressMessage(msg);
        };
    }

    #region Class gen
    /// <summary>
    /// Clears the aliases and removes the selection
    /// </summary>
    private void ClearAlias()
    {
        if (!Tables.Any())
            return;

        foreach (var table in Tables)
        {
            table.Alias = string.Empty;
        }
    }

    /// <summary>
    /// Selects / Deselects the tables
    /// </summary>
    /// <param name="type">The selection type</param>
    private void SelectTables(SelectionType type)
    {
        if (!Tables.Any())
            return;

        foreach (var table in Tables)
        {
            table.Use = type == SelectionType.All;
        }
    }

    /// <summary>
    /// Generates the classes for the selected tables
    /// </summary>
    private async void GenerateClasses()
    {
        InfoList = string.Empty;

        if (_manager == null)
        {
            InfoList = "An error has occurred.";
            return;
        }

        if (!Tables.Any())
        {
            InfoList = "Please select at least on table.";
            return;
        }

        var ctSource = new CancellationTokenSource();
        await ShowProgressAsync("Please wait", "Please wait while creating the classes...", ctSource);

        try
        {
            var options = GetOptions();

            await _manager.GenerateClassesAsync(options, Tables.ToList(), ctSource.Token);
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
            ClassName = string.Empty, // We don't have a class name
            OutputDirectory = ExportDir,
            EmptyDirectoryBeforeExport = EmptyDirBeforeExport,
            Modifier = SelectedModifier,
            SealedClass = OptionSealedClass,
            DbModel = OptionDbModel,
            WithBackingField = OptionBackingField,
            AddSummary = OptionSummary,
            Nullable = OptionNullable,
            AddSetField = OptionSetField,
            Namespace = Namespace
        };
    }
    
    /// <summary>
    /// Filters the list
    /// </summary>
    private void FilterList()
    {
        var tmpResult = string.IsNullOrEmpty(Filter)
            ? _manager!.Tables
            : _manager!.Tables.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();

        Tables = tmpResult.OrderBy(o => o.Name).ToObservableCollection();
    }
    #endregion

    #region Various

    /// <summary>
    /// Shows the info of the set field option
    /// </summary>
    private async void ShowSetFieldInfo()
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
    /// Browse for the export directory
    /// </summary>
    private void BrowseExportDir()
    {
        var dialog = new FolderBrowserDialog
        {
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        ExportDir = dialog.SelectedPath;
    }
    #endregion
}