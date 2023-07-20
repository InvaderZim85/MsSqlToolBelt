using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.Ui.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MsSqlToolBelt.Common.Enums;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.SearchHistoryWindow"/>
/// </summary>
internal partial class SearchHistoryWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the search history
    /// </summary>
    private SearchHistoryManager? _manager;

    /// <summary>
    /// The action to set the selected entry
    /// </summary>
    private Action<string>? _setSelectedEntry;

    /// <summary>
    /// The list with the search history
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SearchHistoryEntry> _searchHistory = new();

    /// <summary>
    /// Backing field for <see cref="SelectedEntry"/>
    /// </summary>
    private SearchHistoryEntry? _selectedEntry;

    /// <summary>
    /// Gets or sets the selected entry
    /// </summary>
    public SearchHistoryEntry? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetProperty(ref _selectedEntry, value) && _manager != null)
                _manager.SelectedEntry = value;
        }
    }

    /// <summary>
    /// The command to set the selected entry
    /// </summary>
    public ICommand SetSelectionCommand => new RelayCommand(() =>
    {
        if (SelectedEntry == null)
            return;

        _setSelectedEntry?.Invoke(SelectedEntry.SearchEntry);
    });

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="manager">The search history manager</param>
    /// <param name="setSelectedEntry">The action to set the selected entry</param>
    public void InitViewModel(SearchHistoryManager manager, Action<string> setSelectedEntry)
    {
        _manager = manager;
        _setSelectedEntry = setSelectedEntry;
    }

    /// <summary>
    /// Sets the list
    /// </summary>
    private void SetList()
    {
        SearchHistory = _manager!.SearchHistory.OrderByDescending(o => o.DateTime).ToObservableCollection();
    }

    /// <summary>
    /// Loads and shows the data
    /// </summary>
    public async void LoadData()
    {
        if (_manager == null)
            return;

        var controller = await ShowProgressAsync("Loading", "Please wait while loading the search history entries...");

        try
        {
            await _manager.LoadSearchHistoryAsync();

            SetList();
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
    /// Deletes the selected entry
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task DeleteEntryAsync()
    {
        if (SelectedEntry == null || _manager == null)
            return;

        var result = await ShowQuestionAsync("Delete",
            $"Do you really want to delete the \"{SelectedEntry.SearchEntry}\" entry?", okButtonText: "Yes",
            cancelButtonText: "No");

        if (result != MessageDialogResult.Affirmative)
            return;

        var controller = await ShowProgressAsync("Deleting", "Please wait while deleting the entry...");

        try
        {
            await _manager.DeleteEntryAsync();

            SetList();
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
    /// Clears the complete history
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        if (_manager == null)
            return;

        var result = await ShowQuestionAsync("Delete", "Do you really want to delete the entire history?",
            okButtonText: "Yes", cancelButtonText: "No");
        if (result != MessageDialogResult.Affirmative)
            return;

        var controller = await ShowProgressAsync("Deleting", "Please wait while clearing the history...");

        try
        {
            await _manager.ClearHistoryAsync();

            SetList();
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
}