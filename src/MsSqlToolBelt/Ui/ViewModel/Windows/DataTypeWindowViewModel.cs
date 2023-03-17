using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Windows;
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
    private ObservableCollection<ClassGenTypeEntry> _dataTypes = new();

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
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Adds a new entry
    /// </summary>
    /// <returns>The awaitable task</returns>
    [RelayCommand]
    private async Task AddEntryAsync()
    {
        var dialog = new DataTypeInputWindow(DataTypes.ToList())
            {Owner = Application.Current.MainWindow};
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
            await ShowErrorAsync(ex);
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

        var dialog = new DataTypeInputWindow(DataTypes.ToList(), SelectedEntry)
            {Owner = Application.Current.MainWindow};

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
            await ShowErrorAsync(ex);
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
            await ShowErrorAsync(ex);
        }
    }
}