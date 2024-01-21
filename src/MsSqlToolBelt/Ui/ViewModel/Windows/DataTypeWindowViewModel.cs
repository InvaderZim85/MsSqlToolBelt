using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.View.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using ZimLabs.Mapper;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.DataTypeWindow"/>
/// </summary>
internal partial class DataTypeWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The list with the data types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ClassGenTypeEntry> _dataTypes = [];

    /// <summary>
    /// The selected entry
    /// </summary>
    [ObservableProperty]
    private ClassGenTypeEntry? _selectedEntry;

    /// <summary>
    /// Loads / shows the data
    /// </summary>
    public async void LoadData()
    {
        try
        {
            DataTypes = ClassGenManager.LoadDataTypes().ToObservableCollection();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
    }

    /// <summary>
    /// Adds a new entry
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task AddEntryAsync()
    {
        var dialog = new DataTypeInputWindow([.. DataTypes])
            { Owner = GetMainWindow() };
        if (dialog.ShowDialog() != true)
            return;

        DataTypes.Add(dialog.Entry);

        try
        {
            // Save the new list
            ClassGenManager.SaveDataTypes(DataTypes);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Edits the selected entry
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task EditEntryAsync()
    {
        if (SelectedEntry == null)
            return;

        // Create a "backup" to reset the value
        var original = SelectedEntry.Clone();

        var dialog = new DataTypeInputWindow([.. DataTypes], SelectedEntry)
            { Owner = GetMainWindow() };

        if (dialog.ShowDialog() != true)
        {
            // Reset the values to it's original
            Mapper.Map(original, SelectedEntry);
            return;
        }

        try
        {
            // Save the values
            ClassGenManager.SaveDataTypes(DataTypes);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }

    /// <summary>
    /// Deletes the selected entry
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task DeleteEntryAsync()
    {
        if (SelectedEntry == null)
            return;

        if (await ShowQuestionAsync("Delete server", $"Do you really want to delete the server '{SelectedEntry.SqlType} <> {SelectedEntry.CSharpType}'?", "Yes", "No") !=
            MessageDialogResult.Affirmative)
            return;

        DataTypes.Remove(SelectedEntry);

        SelectedEntry = null;

        try
        {
            // Save the new list
            ClassGenManager.SaveDataTypes(DataTypes);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
    }
}