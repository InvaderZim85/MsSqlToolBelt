using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Controls;
using Serilog;

namespace MsSqlToolBelt.Ui.View.Controls;

/// <summary>
/// Interaction logic for InfoControl.xaml
/// </summary>
public partial class InfoControl : UserControl
{
    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private SettingsManager? _settingsManager;

    /// <summary>
    /// The start time of the application
    /// </summary>
    private DateTime _startTime;

    /// <summary>
    /// Contains the total up time
    /// </summary>
    private long _totalUpTime;

    /// <summary>
    /// The timer which calculates / shows the total up time
    /// </summary>
    private readonly DispatcherTimer _upTimeTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1)
    };

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
    /// <param name="startTime">The start time of the application</param>
    /// <param name="settingsManager">The instance for the interaction with the settings</param>
    public void InitControl(DateTime startTime, SettingsManager settingsManager)
    {
        _startTime = startTime;
        _settingsManager = settingsManager;

        if (DataContext is InfoControlViewModel viewModel)
            viewModel.LoadData();

        StartUpTimeTimer();
    }

    /// <summary>
    /// Starts the up time timer
    /// </summary>
    private async void StartUpTimeTimer()
    {
        _upTimeTimer.Tick += (_, _) => CalculateUpTime();

        if (_settingsManager == null)
        {
            _upTimeTimer.Start();
            return;
        }

        try
        {
            _totalUpTime = await SettingsManager.LoadSettingsValueAsync<long>(SettingsKey.UpTime);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "An error has occurred while calculating the total up time.");
            LabelUpTime.Content = "00:00:00";
        }
        finally
        {
            _upTimeTimer.Start();
        }
    }

    /// <summary>
    /// Calculates the up time
    /// </summary>
    private void CalculateUpTime()
    {
        var currentUpTime = DateTime.Now - _startTime;
        if (_totalUpTime != 0)
        {
            var totalUpTime = new TimeSpan(_totalUpTime);
            currentUpTime += totalUpTime;
        }

        
        LabelUpTime.Content = currentUpTime.ToFormattedString(true);
    }

    /// <summary>
    /// Stops the timer which calculates the up time
    /// </summary>
    public void StopUpTimeTimer()
    {
        _upTimeTimer.Stop();
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