using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.Internal;
using MsSqlToolBelt.Ui.Common;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.SearchHistoryWindow"/>
/// </summary>
internal class SearchHistoryWindowViewModel : ViewModelBase
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
    /// Backing field for <see cref="SearchHistory"/>
    /// </summary>
    private ObservableCollection<SearchHistoryEntry> _searchHistory = new();

    /// <summary>
    /// Gets or sets the list with the search history
    /// </summary>
    public ObservableCollection<SearchHistoryEntry> SearchHistory
    {
        get => _searchHistory;
        private set => SetProperty(ref _searchHistory, value);
    }

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
    /// The command to delete the selected entry
    /// </summary>
    public ICommand DeleteCommand => new RelayCommand(DeleteEntry);

    /// <summary>
    /// The command to clear the history
    /// </summary>
    public ICommand ClearHistoryCommand => new RelayCommand(ClearHistory);

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

        await ShowProgressAsync("Loading", "Please wait while loading the search history entries...");

        try
        {
            await _manager.LoadSearchHistoryAsync();

            SetList();
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
    /// Deletes the selected entry
    /// </summary>
    private async void DeleteEntry()
    {
        if (SelectedEntry == null || _manager == null)
            return;

        var result = await ShowQuestionAsync("Delete",
            $"Do you really want to delete the \"{SelectedEntry.SearchEntry}\" entry?", okButtonText: "Yes",
            cancelButtonText: "No");

        if (result != MessageDialogResult.Affirmative)
            return;

        await ShowProgressAsync("Deleting", "Please wait while deleting the entry...");

        try
        {
            await _manager.DeleteEntryAsync();

            SetList();
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
    /// Clears the complete history
    /// </summary>
    private async void ClearHistory()
    {
        if (_manager == null)
            return;

        var result = await ShowQuestionAsync("Delete", "Do you really want to delete the entire history?",
            okButtonText: "Yes", cancelButtonText: "No");
        if (result != MessageDialogResult.Affirmative)
            return;

        await ShowProgressAsync("Deleting", "Please wait while clearing the history...");

        try
        {
            await _manager.ClearHistoryAsync();

            SetList();
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
}