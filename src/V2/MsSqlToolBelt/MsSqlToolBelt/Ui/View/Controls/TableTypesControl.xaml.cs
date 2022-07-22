using System.Windows.Controls;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for TableTypesControl.xaml
/// </summary>
public partial class TableTypesControl : UserControl, IConnection
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableTypesControl"/>
    /// </summary>
    public TableTypesControl()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is TableTypesControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        if (DataContext is TableTypesControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public void LoadData()
    {
        if (DataContext is TableTypesControlViewModel viewModel)
            viewModel.LoadData();
    }
}