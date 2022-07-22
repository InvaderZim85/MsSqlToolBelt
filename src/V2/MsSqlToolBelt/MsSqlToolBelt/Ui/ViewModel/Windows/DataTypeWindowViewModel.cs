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
    /// The instance of the class generator
    /// </summary>
    private ClassGenManager? _manager;

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
        set => SetField(ref _dataTypes, value);
    }

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="manager">The instance for the interaction with the class generator</param>
    public void InitViewModel(ClassGenManager? manager)
    {
        _manager = manager;
    }

    /// <summary>
    /// Loads / shows the data
    /// </summary>
    public async void LoadData()
    {
        try
        {
            DataTypes = _manager?.LoadDataTypes().ToObservableCollection() ??
                        new ObservableCollection<ClassGenTypeEntry>();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }
}