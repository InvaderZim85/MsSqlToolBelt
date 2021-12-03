using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.Github;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MetroWindow
    {
        /// <summary>
        /// Contains the release information
        /// </summary>
        private readonly ReleaseInfo _releaseInfo;

        /// <summary>
        /// Creates a new instance of the <see cref="UpdateWindow"/>
        /// </summary>
        /// <param name="releaseInfo">The information of the latest release</param>
        public UpdateWindow(ReleaseInfo releaseInfo)
        {
            InitializeComponent();

            _releaseInfo = releaseInfo;
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        /// <param name="sender">The <see cref="UpdateWindow"/></param>
        /// <param name="e">The event arguments</param>
        private void UpdateWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is UpdateWindowViewModel viewModel)
                viewModel.InitViewModel(_releaseInfo);

            // Set the content of the webbrowser
            WebBrowserInfo.ContextMenu = null;
            WebBrowserInfo.NavigateToString(Helper.MarkdownToHtml(_releaseInfo.Body));
        }
    }
}
