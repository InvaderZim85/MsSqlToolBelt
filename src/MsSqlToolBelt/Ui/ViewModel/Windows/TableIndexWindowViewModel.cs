using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.TableIndexWindow"/>
/// </summary>
internal partial class TableIndexWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the tables
    /// </summary>
    private TableManager? _tableManager;

    /// <summary>
    /// The current table
    /// </summary>
    private TableEntry? _table;

    /// <summary>
    /// The header
    /// </summary>
    [ObservableProperty]
    private string _header = "Table";

    /// <summary>
    /// The list with the indexes
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<IndexEntry> _indexes = new();

    /// <summary>
    /// Gets or sets the value which indicates if the rebuild option should be shown
    /// </summary>
    [ObservableProperty]
    private bool _showRebuildOption;

    /// <summary>
    /// Gets the value which indicates if the load fragmentation button should be shown
    /// </summary>
    public bool ShowLoadFragmentationButton => !ShowRebuildOption;

    /// <summary>
    /// Gets or sets the fill factor
    /// </summary>
    [ObservableProperty]
    private int _fillFactor;

    /// <summary>
    /// Occurs when <see cref="ShowRebuildOption"/> was changed
    /// </summary>
    /// <param name="value">The new value</param>
    partial void OnShowRebuildOptionChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowLoadFragmentationButton));
    }

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="tableManager">The instance for the interaction with the tables</param>
    /// <param name="table">The desired table</param>
    public void InitViewModel(TableManager tableManager, TableEntry table)
    {
        _tableManager = tableManager;
        _table = table;
        
        Header = table.Name;
        SetIndexes();
    }

    /// <summary>
    /// Sets the index list
    /// </summary>
    private void SetIndexes()
    {
        Indexes = (_table?.Indexes ?? new List<IndexEntry>()).ToObservableCollection();
    }

    /// <summary>
    /// Loads the fragmentation of the table indizes
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task LoadTableFragmentationAsync()
    {
        if (_tableManager == null || _table == null)
            return;

        var controller = await ShowProgressAsync("Please wait",
            "Please wait while loading the table fragmentation data (this may take a while)...");

        try
        {
            await LoadTableIndexFragmentationAsync();

            ShowRebuildOption = true;
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
    /// Rebuilds the indexes of the table
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task RebuildIndexesAsync()
    {
        if (_tableManager == null || _table == null)
            return;

        if (await ShowMessageWithOptionAsync(new MessageEntry("Rebuild",
                "Before you continue. No guarantee can be given that the function will work correctly and/or that any other errors may occur.",
                "",
                "Are you aware of the consequences?",
                "",
                "If you are not sure, please read the linked description.")) != MessageDialogResult.Affirmative)
            return;

        var controller = await ShowProgressAsync("Please wait",
            "Please wait while loading the table fragmentation data (this may take a while)...");

        try
        {
            // Rebuild the indexes
            await Task.Run(() => _tableManager.RebuildTableIndexes(_table, FillFactor));

            // Reload the fragmentation
            await LoadTableIndexFragmentationAsync();
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
    /// Loads the table index fragmentation
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task LoadTableIndexFragmentationAsync()
    {
        await Task.Run(() => _tableManager!.LoadTableIndexFragmentation(_table!));

        SetIndexes();
    }
}