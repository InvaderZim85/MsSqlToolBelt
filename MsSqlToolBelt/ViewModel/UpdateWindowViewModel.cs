using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MsSqlToolBelt.DataObjects.Github;
using MsSqlToolBelt.View;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the logic for the <see cref="View.UpdateWindow"/>
    /// </summary>
    internal sealed class UpdateWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Contains the download url
        /// </summary>
        private string _downloadUrl;

        /// <summary>
        /// The name of the file
        /// </summary>
        private string _fileName;

        /// <summary>
        /// Backing field for <see cref="ReleaseInfo"/>
        /// </summary>
        private ReleaseInfo _releaseInfo;

        /// <summary>
        /// Gets or sets the release information
        /// </summary>
        public ReleaseInfo ReleaseInfo
        {
            get => _releaseInfo;
            private set => SetField(ref _releaseInfo, value);
        }

        /// <summary>
        /// Backing field for <see cref="FileSize"/>
        /// </summary>
        private string _fileSize;

        /// <summary>
        /// Gets or sets the size of the zip archive
        /// </summary>
        public string FileSize
        {
            get => _fileSize;
            private set => SetField(ref _fileSize, value);
        }

        /// <summary>
        /// Backing field for <see cref="PublishedAt"/>
        /// </summary>
        private string _publishedAt;

        /// <summary>
        /// Gets or sets the publish date
        /// </summary>
        public string PublishedAt
        {
            get => _publishedAt;
            private set => SetField(ref _publishedAt, value);
        }

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
            FileSize = asset.Size.ConvertSize();
            PublishedAt = releaseInfo.PublishedAt.ToString("yyyy-MM-dd HH:mm:ss");
            _fileName = asset.Name;
        }

        /// <summary>
        /// The command to open github
        /// </summary>
        public ICommand OpenGitHubCommand => new DelegateCommand(OpenGithub);

        /// <summary>
        /// The command to start the download
        /// </summary>
        public ICommand StartDownloadCommand => new DelegateCommand(StartDownload);

        /// <summary>
        /// Opens the page on github
        /// </summary>
        private void OpenGithub()
        {
            if (string.IsNullOrWhiteSpace(ReleaseInfo.HtmlUrl))
                return;

            // Open the website in your default browser
            Process.Start(ReleaseInfo.HtmlUrl);
        }

        /// <summary>
        /// Starts the download
        /// </summary>
        private void StartDownload()
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

            var downloadDialog = new DownloadDialog(_downloadUrl, dialog.FileName)
            {
                Owner = Application.Current.MainWindow
            };

            downloadDialog.ShowDialog();
        }
    }
}
