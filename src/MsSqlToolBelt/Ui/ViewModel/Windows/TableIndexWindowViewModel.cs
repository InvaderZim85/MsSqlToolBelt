using System.Collections.ObjectModel;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.DataObjects.Search;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.TableIndexWindow"/>
/// </summary>
internal class TableIndexWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="Header"/>
    /// </summary>
    private string _header = "Table";

    /// <summary>
    /// Gets or sets the header
    /// </summary>
    public string Header
    {
        get => _header;
        private set => SetProperty(ref _header, value);
    }
    /// <summary>
    /// Backing field for <see cref="Indexes"/>
    /// </summary>
    private ObservableCollection<IndexEntry> _indexes = new();

    /// <summary>
    /// Gets or sets the list with the indexes
    /// </summary>
    public ObservableCollection<IndexEntry> Indexes
    {
        get => _indexes;
        private set => SetProperty(ref _indexes, value);
    }

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