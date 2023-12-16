using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Business;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using System.Collections.ObjectModel;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.ServerInfoWindow"/>
/// </summary>
internal partial class ServerInfoWindowViewModel : ViewModelBase
{
    /// <summary>
    /// The instance for the interaction with the base manager
    /// </summary>
    private BaseManager? _manager;

    /// <summary>
    /// Gets or sets the list with the server info
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ServerInfoEntry> _serverInfo = [];

    /// <summary>
    /// Gets or sets the selected server info
    /// </summary>
    [ObservableProperty]
    private ServerInfoEntry? _selectedServerInfo;

    /// <summary>
    /// Gets or sets the sub list with the server info
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ServerInfoEntry> _subInfoList = [];

    /// <summary>
    /// Gets or sets the filter
    /// </summary>
    [ObservableProperty]
    private string _filter = string.Empty;

    /// <summary>
    /// Occurs when the user selects another server info entry
    /// </summary>
    /// <param name="value">The selected value</param>
    partial void OnSelectedServerInfoChanged(ServerInfoEntry? value)
    {
        SubInfoList = (value?.ChildValues ?? []).ToObservableCollection();
    }

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="manager">The base manager</param>
    public void InitViewModel(BaseManager manager)
    {
        _manager = manager;
    }

    /// <summary>
    /// Loads the server info
    /// </summary>
    public async void LoadServerInfo()
    {
        if (_manager == null)
            return;

        var controller = await ShowProgressAsync("Please wait", "Please wait while loading the server information...");

        try
        {
            await _manager.LoadServerInformationAsync();

            FilterList();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Load);
        }
        finally
        {
            await controller.CloseAsync();
        }
    }

    /// <summary>
    /// Filters the list
    /// </summary>
    [RelayCommand]
    private void FilterList()
    {
        if (_manager == null)
            return;

        var tmpList = string.IsNullOrEmpty(Filter)
            ? _manager.ServerInfo
            : _manager.ServerInfo.Where(w => w.Key.ContainsIgnoreCase(Filter) ||
                                             w.Value.ContainsIgnoreCase(Filter));

        ServerInfo = tmpList.OrderBy(o => o.Order).ThenBy(t => t.Key).ToObservableCollection();
    }



    /// <summary>
    /// Copies / Exports the table information
    /// </summary>
    [RelayCommand]
    private void CopyExportTable()
    {
        var tmpList = ServerInfo.Where(w => w.ChildValues.Count == 0).ToList();
        if (tmpList.Count == 0)
            return;

        ExportListData(tmpList, "ServerInfo");
    }
}