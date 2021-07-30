using System;
using System.Windows;
using CommandLine;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects;
using MsSqlToolBelt.DataObjects.Types;
using MsSqlToolBelt.ViewModel;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Contains the provided arguments
        /// </summary>
        private readonly string[] _args;

        /// <summary>
        /// Creates a new instance of the <see cref="MainWindow"/>
        /// </summary>
        public MainWindow(string[] args)
        {
            InitializeComponent();
            
            _args = args;
        }

        /// <summary>
        /// Sets the connector of the search and table type control
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        private void SetConnector(Connector connector)
        {
            SearchControl.SetConnector(connector);
            TableTypeControl.SetConnector(connector);
            ClassGeneratorControl.SetConnector(connector);

            LoadData(TabControl.SelectedIndex);
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.InitViewModel(SetConnector, LoadData, ClearControl, InitFlyOut);

            SearchControl.InitControl();
            ClassGeneratorControl.InitControl();
        }

        /// <summary>
        /// Loads the data of the desired control
        /// </summary>
        /// <param name="tabIndex">The tab index</param>
        private void LoadData(int tabIndex)
        {
            switch (tabIndex)
            {
                case 1:
                    TableTypeControl.LoadData();
                    break;
                case 2:
                    ClassGeneratorControl.LoadData();
                    break;
            }
        }

        /// <summary>
        /// Clears the content of the controls
        /// </summary>
        private void ClearControl()
        {
            SearchControl.Clear();
            TableTypeControl.Clear();
            ClassGeneratorControl.Clear();
        }

        /// <summary>
        /// Init the fly out
        /// </summary>
        /// <param name="type">The type of the fly out</param>
        private void InitFlyOut(FlyOutType type)
        {
            switch (type)
            {
                case FlyOutType.Settings:
                    _settingsControl.InitControl();
                    break;
                case FlyOutType.DataTypes:
                    _dataTypeControl.InitControl();
                    break;
                case FlyOutType.Info:
                    _infoControl.InitControl();
                    break;
            }
        }

        /// <summary>
        /// Occurs when the content was rendered
        /// </summary>
        /// <param name="sender">The <see cref="MainWindow"/></param>
        /// <param name="e">The event arguments</param>
        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            if (_args == null)
                return;

            if (DataContext is not MainWindowViewModel viewModel)
                return;

            Parser.Default.ParseArguments<Arguments>(_args).WithParsed(a => viewModel.AutoConnect(a));
        }

        /// <summary>
        /// Occurs when the fly out was closed
        /// </summary>
        /// <param name="sender">The settings fly out</param>
        /// <param name="e">The event arguments</param>
        private void Flyout_OnClosingFinished(object sender, RoutedEventArgs e)
        {
            _settingsControl?.SetThemeDefault();
        }
    }
}
