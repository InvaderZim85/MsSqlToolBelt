﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.View.Windows;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Ui.ViewModel.Controls;

/// <summary>
/// Provides the logic for the <see cref="View.Controls.InfoControl"/>
/// </summary>
internal class InfoControlViewModel : ViewModelBase
{
    /// <summary>
    /// Contains the value which indicates if the data were already loaded
    /// </summary>
    private bool _dataLoaded;

    /// <summary>
    /// Backing field for <see cref="LogDir"/>
    /// </summary>
    private string _logDir = string.Empty;

    /// <summary>
    /// Gets or sets the path of the directory which contains the log files
    /// </summary>
    public string LogDir
    {
        get => _logDir;
        set => SetProperty(ref _logDir, value);
    }

    /// <summary>
    /// Backing field for <see cref="VersionInfo"/>
    /// </summary>
    private string _versionInfo = string.Empty;

    /// <summary>
    /// Gets or sets the info about the version
    /// </summary>
    public string VersionInfo
    {
        get => _versionInfo;
        private set => SetProperty(ref _versionInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="BuildInfo"/>
    /// </summary>
    private string _buildInfo = string.Empty;

    /// <summary>
    /// Gets or sets the build info
    /// </summary>
    public string BuildInfo
    {
        get => _buildInfo;
        private set => SetProperty(ref _buildInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="PackageList"/>
    /// </summary>
    private ObservableCollection<PackageInfo> _packageList = new();

    /// <summary>
    /// Gets or sets the list with the used packages
    /// </summary>
    public ObservableCollection<PackageInfo> PackageList
    {
        get => _packageList;
        private set => SetProperty(ref _packageList, value);
    }

    /// <summary>
    /// The command to open the log dir
    /// </summary>
    public ICommand OpenLogDirCommand => new RelayCommand(() =>
    {
        if (string.IsNullOrEmpty(LogDir) || !Directory.Exists(LogDir))
            return;

        Helper.ShowInExplorer(LogDir);
    });

    /// <summary>
    /// Loads / shows the data
    /// </summary>
    public async void LoadData()
    {
        if (_dataLoaded)
            return;

        LogDir = Path.Combine(Core.GetBaseDirPath(), "logs");

        var packageFile = Path.Combine(Core.GetBaseDirPath(), "Packages.csv");
        if (!File.Exists(packageFile))
            return;

        var (versionInfo, buildInfo) = Helper.GetVersionInfo();
        VersionInfo = versionInfo;
        BuildInfo = buildInfo;

        try
        {
            await ExtractPackageInformationAsync(packageFile);

            _dataLoaded = true;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
    }

    /// <summary>
    /// Extracts the package information 
    /// </summary>
    /// <param name="filepath">The path of the csv file which contains the packages</param>
    /// <returns>The awaitable task</returns>
    private async Task ExtractPackageInformationAsync(string filepath)
    {
        var content = await File.ReadAllLinesAsync(filepath);
        var tmpList = (from line in content
            select line.Split(new[] {";"}, StringSplitOptions.TrimEntries)
            into lineContent
            where lineContent.Length == 2
            select new PackageInfo {Name = lineContent[0], Version = lineContent[1]}).ToList();

        PackageList = tmpList.OrderBy(o => o.Name).ToObservableCollection();
    }
}