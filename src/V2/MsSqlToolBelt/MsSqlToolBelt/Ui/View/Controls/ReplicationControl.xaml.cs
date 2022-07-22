using System.Windows.Controls;
using MsSqlToolBelt.Ui.View.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for ReplicationControl.xaml
/// </summary>
public partial class ReplicationControl : UserControl, IConnection
{
    /// <summary>
    /// Creates a new instance of the <see cref="ReplicationControl"/>
    /// </summary>
    public ReplicationControl()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    public void SetConnection(string dataSource, string database)
    {
        if (DataContext is ReplicationControlViewModel viewModel)
            viewModel.SetConnection(dataSource, database);
    }

    /// <inheritdoc />
    public void CloseConnection()
    {
        if (DataContext is ReplicationControlViewModel viewModel)
            viewModel.CloseConnection();
    }

    /// <summary>
    /// Loads the data
    /// </summary>
    public void LoadData()
    {
        if (DataContext is ReplicationControlViewModel viewModel)
            viewModel.LoadData();
    }
}