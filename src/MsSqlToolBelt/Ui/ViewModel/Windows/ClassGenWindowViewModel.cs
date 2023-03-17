using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
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
internal partial class ClassGenWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the class generator
    /// </summary>
    private ClassGenManager? _manager;

    #region View properties

    /// <summary>
    /// The list with the tables
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TableDto> _tables = new();

    /// <summary>
    /// The info
    /// </summary>
    [ObservableProperty]
    private string _infoList = string.Empty;

    /// <summary>
    /// The list with the modifier
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _modifierList = new();

    /// <summary>
    /// The selected modifier
    /// </summary>
    [ObservableProperty]
    private string _selectedModifier = "public";

    /// <summary>
    /// The value which indicates if a sealed class should be created
    /// </summary>
    [ObservableProperty]
    private bool _optionSealedClass;

    /// <summary>
    /// The value which indicates if a DB model should be created
    /// </summary>
    [ObservableProperty]
    private bool _optionDbModel;

    /// <summary>
    /// The value which indicates if a backing field should be created
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
    /// The value which indicates if a summary should be added
    /// </summary>
    [ObservableProperty]
    private bool _optionSummary;

    /// <summary>
    /// The value which indicates if the nullable property (.NET 6) should be used
    /// </summary>
    [ObservableProperty]
    private bool _optionNullable;

    /// <summary>
    /// The desired namespace
    /// </summary>
    [ObservableProperty]
    private string _namespace = string.Empty;

    /// <summary>
    /// The path of the export directory
    /// </summary>
    [ObservableProperty]
    private string _exportDir = string.Empty;

    /// <summary>
    /// The value which indicates if the export directory should be cleared before export
    /// </summary>
    [ObservableProperty]
    private bool _emptyDirBeforeExport;

    /// <summary>
    /// The header for the list
    /// </summary>
    [ObservableProperty]
    private string _headerTables = "Tables";

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

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="classGenManager">The instance of the class generator</param>
    public void InitViewModel(ClassGenManager classGenManager)
    {
        _manager = classGenManager;

        FilterList();

        ModifierList = ClassGenManager.GetModifierList();
        SelectedModifier = "public";
    }

    #region Class gen
    /// <summary>
    /// Clears the aliases and removes the selection
    /// </summary>
    [RelayCommand]
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
    [RelayCommand]
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
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task GenerateClassesAsync()
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
        var controller = await ShowProgressAsync("Please wait", "Please wait while creating the classes...", ctSource);

        try
        {
            var options = GetOptions();

            _manager.Progress += (_, msg) => controller.SetMessage(msg);

            await _manager.GenerateClassesAsync(options, Tables.ToList(), ctSource.Token);
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
    [RelayCommand]
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
    /// Browse for the export directory
    /// </summary>
    [RelayCommand]
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