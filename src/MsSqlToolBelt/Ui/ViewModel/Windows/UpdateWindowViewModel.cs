using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.Updater;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the 
/// </summary>
internal class UpdateWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Contains the download url
    /// </summary>
    private string _downloadUrl = string.Empty;

    /// <summary>
    /// Contains the name of the file (needed for the download)
    /// </summary>
    private string _fileName = string.Empty;

    /// <summary>
    /// Contains the path of the download file
    /// </summary>
    private string _filepath = string.Empty;

    /// <summary>
    /// Backing field for <see cref="ReleaseInfo"/>
    /// </summary>
    private ReleaseInfo _releaseInfo = new();

    /// <summary>
    /// Gets or sets the release info
    /// </summary>
    public ReleaseInfo ReleaseInfo
    {
        get => _releaseInfo;
        private set => SetProperty(ref _releaseInfo, value);
    }

    /// <summary>
    /// Backing field for <see cref="FileSize"/>
    /// </summary>
    private string _fileSize = string.Empty;

    /// <summary>
    /// Gets or sets the size of the release file in a readable format
    /// </summary>
    public string FileSize
    {
        get => _fileSize;
        private set => SetProperty(ref _fileSize, value);
    }

    /// <summary>
    /// Backing field for <see cref="ButtonOpenFileEnabled"/>
    /// </summary>
    private bool _buttonOpenFileEnabled;

    /// <summary>
    /// Gets or sets the value which indicates if the open file button is enabled
    /// </summary>
    public bool ButtonOpenFileEnabled
    {
        get => _buttonOpenFileEnabled;
        set => SetProperty(ref _buttonOpenFileEnabled, value);
    }

    /// <summary>
    /// Backing field for <see cref="BrowserVisibility"/>
    /// </summary>
    private Visibility _browserVisibility = Visibility.Visible;

    /// <summary>
    /// Gets or sets the value which indicates if the webbrowser is visible
    /// </summary>
    public Visibility BrowserVisibility
    {
        get => _browserVisibility;
        set => SetProperty(ref _browserVisibility, value);
    }

    /// <summary>
    /// The command to open the git link
    /// </summary>
    public ICommand OpenGitCommand => new RelayCommand(OpenGithub);

    /// <summary>
    /// The command to start the download
    /// </summary>
    public ICommand DownloadCommand => new RelayCommand(StartDownload);

    /// <summary>
    /// The command to open the downloaded file
    /// </summary>
    public ICommand OpenFileCommand => new RelayCommand(() =>
    {
        if (string.IsNullOrEmpty(_filepath) || !File.Exists(_filepath))
            return;

        Helper.ShowInExplorer(_filepath);
    });

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="releaseInfo">The release info</param>
    public void InitViewModel(ReleaseInfo releaseInfo)
    {
        ReleaseInfo = releaseInfo;

        var asset = releaseInfo.Assets.FirstOrDefault();
        if (asset == null)
            return;

        _downloadUrl = asset.DownloadUrl;
        _fileName = asset.Name;
        FileSize = asset.SizeView;
    }

    /// <summary>
    /// Opens the page on github
    /// </summary>
    private void OpenGithub()
    {
        if (string.IsNullOrWhiteSpace(ReleaseInfo.HtmlUrl))
            return;

        Helper.OpenLink(ReleaseInfo.HtmlUrl);
    }

    /// <summary>
    /// Starts the download
    /// </summary>
    private async void StartDownload()
    {
        if (string.IsNullOrWhiteSpace(_downloadUrl))
            return;

        var dialog = new SaveFileDialog
        {
            Title = "Save as...",
            Filter = "ZIP archive|*.zip",
            FileName = _fileName
        };

        if (dialog.ShowDialog() != true)
            return;

        _filepath = dialog.FileName;
        // We need to hide the browser, because the dialog overlay is behind the browser ....
        BrowserVisibility = Visibility.Hidden;

        var controller = await ShowProgressAsync("Please wait", "Please wait while downloading the file...");

        try
        {
            var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(_downloadUrl);
            await File.WriteAllBytesAsync(_filepath, bytes);

            ButtonOpenFileEnabled = File.Exists(_filepath);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            await controller.CloseAsync();
            BrowserVisibility = Visibility.Visible;
        }
    }

}