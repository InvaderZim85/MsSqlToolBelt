using System.Windows.Controls;
using MsSqlToolBelt.Ui.ViewModel.Controls;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for SettingsControl.xaml
/// </summary>
public partial class SettingsControl : UserControl
{
    /// <summary>
    /// Creates a new instance of the <see cref="SettingsControl"/>
    /// </summary>
    public SettingsControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Init the control
    /// </summary>
    public void InitControl()
    {
        if (DataContext is SettingsControlViewModel viewModel)
            viewModel.InitViewModel();
    }
}