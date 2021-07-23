using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
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
                viewModel.InitViewModel();
        }

        /// <summary>
        /// Occurs when the user hits the link
        /// </summary>
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}
