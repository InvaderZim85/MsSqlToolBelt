using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.TableIndexWindow"/>
/// </summary>
internal partial class TableIndexWindowViewModel : ViewModelBase
{
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
    /// Init the view model
    /// </summary>
    /// <param name="table">The desired table</param>
    public void InitViewModel(TableEntry table)
    {
        Header = table.Name;
        Indexes = new ObservableCollection<IndexEntry>(table.Indexes);
    }
}