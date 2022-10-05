using System;
using System.Collections.ObjectModel;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.Common;

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
        private set => SetField(ref _dataTypes, value);
    }

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
}