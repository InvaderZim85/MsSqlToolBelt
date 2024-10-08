﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.View.Windows;
using System.Collections.ObjectModel;
using System.Windows.Forms;
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
    private ObservableCollection<TableDto> _tables = [];

    /// <summary>
    /// The info
    /// </summary>
    [ObservableProperty]
    private string _infoList = string.Empty;

    /// <summary>
    /// The list with the modifier
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _modifierList = [];

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
    /// Gets or sets the value which indicates if the column property should be added
    /// <para />
    /// Only for EF Core properties
    /// </summary>
    [ObservableProperty]
    private bool _optionColumnAttribute;

    /// <summary>
    /// Gets or sets the value which indicates if a backing field should be created
    /// </summary>
    [ObservableProperty]
    private bool _optionBackingField;

    /// <summary>
    /// Occurs when the value of the <see cref="OptionBackingField"/> was changed
    /// </summary>
    /// <param name="value"></param>
    partial void OnOptionBackingFieldChanged(bool value)
    {
        if (!value)
            OptionSetField = false;
    }

    /// <summary>
    /// Gets or sets the value which indicates if the set field method should be used.
    /// <para />
    /// If this option is enabled, the option <see cref="OptionBackingField"/> will be enabled)
    /// </summary>
    [ObservableProperty]
    private bool _optionSetField;

    /// <summary>
    /// Occurs when the value of <see cref="OptionSetField"/> was changed
    /// </summary>
    /// <param name="value">The new value</param>
    partial void OnOptionSetFieldChanged(bool value)
    {
        if (value)
            OptionBackingField = true;
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
    /// Gets or sets the value which indicates whether the summary should be added in the summary
    /// </summary>
    [ObservableProperty]
    private bool _optionAddTableNameInSummary;

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
    /// Gets or sets the filter
    /// </summary>
    [ObservableProperty]
    private string _filter = string.Empty;

    /// <summary>
    /// Occurs when the value of <see cref="Filter"/> was changed
    /// </summary>
    /// <param name="value">The new filter value</param>
    partial void OnFilterChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            FilterList();
    }

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

    #endregion

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="classGenManager">The instance of the class generator</param>
    public void InitViewModel(ClassGenManager classGenManager)
    {
        _manager = classGenManager;

        FilterList();

        ModifierList = CommonValues.GetModifierList();
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

            _manager.Progress += (_, msg) =>
            {
                controller.SetMessage(msg);
                InfoList += $"{Environment.NewLine}{msg}";
            };

            await _manager.GenerateClassesAsync(options, [.. Tables], ctSource.Token);
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
            ClassName = string.Empty, // We don't have a class name
            OutputDirectory = ExportDir,
            EmptyDirectoryBeforeExport = EmptyDirBeforeExport,
            Modifier = SelectedModifier,
            SealedClass = OptionSealedClass,
            DbModel = OptionDbModel,
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