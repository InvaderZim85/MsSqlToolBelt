using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.Updater;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for UpdateWindow.xaml
/// </summary>
public partial class UpdateWindow : MetroWindow
{
    /// <summary>
    /// Contains the release info
    /// </summary>
    private readonly ReleaseInfo _releaseInfo;

    /// <summary>
    /// Creates a new instance of the <see cref="UpdateWindow"/>
    /// </summary>
    /// <param name="releaseInfo">The release info</param>
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

    /// <summary>
    /// Occurs when the user hits the close button
    /// </summary>
    /// <param name="sender">The <see cref="ButtonClose"/></param>
    /// <param name="e">The event arguments</param>
    private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}