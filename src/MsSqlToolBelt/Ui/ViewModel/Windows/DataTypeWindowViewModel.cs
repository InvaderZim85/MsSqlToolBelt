using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
internal class DataTypeWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="DataTypes"/>
    /// </summary>
    private ObservableCollection<ClassGenTypeEntry> _dataTypes = new();

    /// <summary>
    /// Gets or sets the list with the data types
    /// </summary>
    public ObservableCollection<ClassGenTypeEntry> DataTypes
    {
        get => _dataTypes;
        private set => SetProperty(ref _dataTypes, value);
    }

    /// <summary>
    /// Backing field for <see cref="SelectedEntry"/>
    /// </summary>
    private ClassGenTypeEntry? _selectedEntry;

    /// <summary>
    /// Gets or sets the selected entry
    /// </summary>
    public ClassGenTypeEntry? SelectedEntry
    {
        get => _selectedEntry;
        set => SetProperty(ref _selectedEntry, value);
    }

    /// <summary>
    /// The command which occurs when the user hits the add button
    /// </summary>
    public ICommand AddCommand => new RelayCommand(AddEntry);

    /// <summary>
    /// The command which occurs when the user hits the edit button
    /// </summary>
    public ICommand EditCommand => new RelayCommand(EditEntry);

    /// <summary>
    /// The command which occurs when the user hits the delete button
    /// </summary>
    public ICommand DeleteCommand => new RelayCommand(DeleteEntry);

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
    private async void AddEntry()
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
    private async void EditEntry()
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
    private async void DeleteEntry()
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