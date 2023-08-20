using MahApps.Metro.Controls;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using System;
using System.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for ServerInfoWindow.xaml
/// </summary>
public partial class ServerInfoWindow : MetroWindow
{
    /// <summary>
    /// The instance of the base manager
    /// </summary>
    private readonly BaseManager _manager;

    /// <summary>
    /// Creates a new instance of the <see cref="ServerInfoWindow"/>
    /// </summary>
    /// <param name="manager">The instance of the base manager</param>
    public ServerInfoWindow(BaseManager manager)
    {
        InitializeComponent();

        _manager = manager;
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="ServerInfoWindow"/></param>
    private void ServerInfoWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ServerInfoWindowViewModel viewModel)
            viewModel.InitViewModel(_manager);
    }

    /// <summary>
    /// Occurs when the window was rendered
    /// </summary>
    /// <param name="sender">The <see cref="ServerInfoWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void ServerInfoWindow_OnContentRendered(object? sender, EventArgs e)
    {
        if (DataContext is ServerInfoWindowViewModel viewModel)
            viewModel.LoadServerInfo();
    }
}