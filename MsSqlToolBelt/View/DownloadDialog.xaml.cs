using System;
using System.IO;
using System.Net;
using System.Windows;
using MahApps.Metro.Controls;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for DownloadDialog.xaml
    /// </summary>
    public partial class DownloadDialog : MetroWindow
    {
        /// <summary>
        /// Contains the url file which should be downloaded
        /// </summary>
        private readonly string _url;

        /// <summary>
        /// The path of the file
        /// </summary>
        private readonly string _filepath;

        /// <summary>
        /// Creates a new instance of the <see cref="DownloadDialog"/>
        /// </summary>
        /// <param name="url">The url of the file which should be downloaded</param>
        /// <param name="filepath">The path where the file should be downloaded</param>
        public DownloadDialog(string url, string filepath)
        {
            InitializeComponent();

            _url = url;
            _filepath = filepath;

            LabelHeadline.Content = Path.GetFileName(filepath);
        }

        /// <summary>
        /// Downloads the file
        /// </summary>
        private void Download()
        {
            var client = new WebClient();
            client.DownloadFileAsync(new Uri(_url), _filepath);

            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += (_, _) =>
            {
                ButtonClose.IsEnabled = true;
                ButtonOpen.IsEnabled = true;
                LabelDownloadProcess.Content = "Download completed";
            };
        }

        /// <summary>
        /// Occurs when the progress changed
        /// </summary>
        /// <param name="sender">The web client"/></param>
        /// <param name="e">The event arguments</param>
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar.Maximum = e.TotalBytesToReceive;
            ProgressBar.Value = e.BytesReceived;

            LabelDownloadProcess.Content = $"{e.BytesReceived.ConvertSize()} / {e.TotalBytesToReceive.ConvertSize()} {e.ProgressPercentage}%";
            LabelDownloadProcessPercentage.Content = $"{e.ProgressPercentage}%";
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        /// <param name="sender">The <see cref="DownloadDialog"/></param>
        /// <param name="e">The event arguments</param>
        private void DownloadDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            Download();
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

        /// <summary>
        /// Occurs when the user hits the open button
        /// </summary>
        /// <param name="sender">The open button</param>
        /// <param name="e">The event arguments</param>
        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            Helper.ShowInExplorer(_filepath);
        }
    }
}
