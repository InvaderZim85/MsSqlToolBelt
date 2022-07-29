using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for InfoControl.xaml
/// </summary>
public partial class InfoControl : UserControl
{
    /// <summary>
    /// Creates a new instance of the <see cref="InfoControl"/>
    /// </summary>
    public InfoControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Init the control
    /// </summary>
    public void InitControl()
    {
        if (DataContext is InfoControlViewModel viewModel)
            viewModel.LoadData();
    }

    /// <summary>
    /// Occurs when the user clicks the hyperlink of the project file
    /// </summary>
    /// <param name="sender">The project link"/></param>
    /// <param name="e">The event arguments</param>
    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Helper.OpenLink(e.Uri.ToString());
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridPackages"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridPackages_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridPackages.CopyToClipboard<PackageInfo>();
    }
}