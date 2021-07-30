using System.Windows.Controls;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
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

        /// <summary>
        /// Sets the theme to the saved values
        /// </summary>
        public void SetThemeDefault()
        {
            if (DataContext is SettingsControlViewModel viewModel)
                viewModel.ChangeTheme(true);
        }
    }
}
