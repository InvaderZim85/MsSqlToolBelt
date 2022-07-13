﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Ui.View.Windows;
using Serilog;
using ZimLabs.WpfBase.NetCore;

namespace MsSqlToolBelt.Ui.ViewModel
{
    /// <summary>
    /// Provides the base functions of a view model
    /// </summary>
    internal class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// The instance of the mah apps dialog coordinator
        /// </summary>
        private readonly IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// The message timer
        /// </summary>
        private readonly Timer _messageTimer = new(TimeSpan.FromSeconds(10).TotalMilliseconds);

        /// <summary>
        /// Backing field for <see cref="InfoMessage"/>
        /// </summary>
        private string _infoMessage = string.Empty;

        /// <summary>
        /// Gets or sets the message which should be shown
        /// </summary>
        public string InfoMessage
        {
            get => _infoMessage;
            set => SetField(ref _infoMessage, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ViewModelBase"/>
        /// </summary>
        protected ViewModelBase()
        {
            _dialogCoordinator = DialogCoordinator.Instance;

            _messageTimer.Elapsed += (_, _) =>
            {
                InfoMessage = string.Empty;
                _messageTimer.Stop();
            };
        }

        /// <summary>
        /// Shows a message dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <returns>The awaitable task</returns>
        protected async Task ShowMessageAsync(string title, string message)
        {
            await _dialogCoordinator.ShowMessageAsync(this, title, message);
        }

        /// <summary>
        /// Shows a question dialog with two buttons
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="okButtonText">The text of the ok button (optional)</param>
        /// <param name="cancelButtonText">The text of the cancel button (optional)</param>
        /// <returns>The dialog result</returns>
        protected async Task<MessageDialogResult> ShowQuestionAsync(string title, string message, string okButtonText = "OK",
            string cancelButtonText = "Cancel")
        {
            var result = await _dialogCoordinator.ShowMessageAsync(this, title, message,
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = okButtonText,
                    NegativeButtonText = cancelButtonText
                });

            return result;
        }

        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="ex">The exception which was thrown</param>
        /// <param name="caller">The name of the method, which calls this method. Value will be filled automatically</param>
        /// <returns>The awaitable task</returns>
        protected async Task ShowErrorAsync(Exception ex, [CallerMemberName] string caller = "")
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occurred: {ex.Message}");
            LogError(ex, caller);
        }

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="ex">The exception which was thrown</param>
        /// <param name="caller">The name of the method, which calls this method. Value will be filled automatically</param>
        protected void LogError(Exception ex, [CallerMemberName] string caller = "")
        {
            Log.Error(ex, "An error has occurred. Caller: {caller}", caller);
        }

        /// <summary>
        /// Shows a progress dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="setIndeterminate">true to set the controller to indeterminate, otherwise false (optional)</param>
        /// <returns>The progress controller</returns>
        protected async Task<ProgressDialogController> ShowProgressAsync(string title, string message, bool setIndeterminate = true)
        {
            var controller = await _dialogCoordinator.ShowProgressAsync(this, title, message);
            if (setIndeterminate)
                controller.SetIndeterminate();

            return controller;
        }

        /// <summary>
        /// Shows a input dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <returns>The result of the input</returns>
        protected async Task<string> ShowInputAsync(string title, string message)
        {
            var result = await _dialogCoordinator.ShowInputAsync(this, title, message);

            return result;
        }

        /// <summary>
        /// Shows an info message for 10 seconds
        /// </summary>
        /// <param name="message">The message which should be shown</param>
        protected void ShowInfoMessage(string message)
        {
            InfoMessage = message;
            _messageTimer.Start();
        }

        /// <summary>
        /// Copies the content to the clipboard
        /// </summary>
        /// <param name="content">The content which should be copied</param>
        protected void CopyToClipboard(string content)
        {
            Clipboard.SetText(content);
        }

        /// <summary>
        /// Exports the desired data 
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="data">The data which should be exported</param>
        /// <param name="defaultName">The default name of the file</param>
        protected async void ExportObjectData<T>(T data, string defaultName) where T : class
        {
            ProgressDialogController? controller = null;
            try
            {
                var dialog = new ExportWindow(defaultName, ExportDataType.Single);
                if (dialog.ShowDialog() != true)
                    return;

                if (dialog.Export)
                {
                    controller = await ShowProgressAsync("Please wait", "Please wait while export the data...");

                    await ExportHelper.ExportObject(data, dialog.FileName, dialog.ExportType);
                }
                else
                {
                    var content = ExportHelper.CreateObjectContent(data, dialog.ExportType);
                    CopyToClipboard(content);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex);
            }
            finally
            {
                if (controller != null)
                    await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Exports the desired list
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="data">The data which should be exported</param>
        /// <param name="defaultName">The default name of the file</param>
        protected async void ExportListData<T>(IEnumerable<T> data, string defaultName) where T : class
        {
            ProgressDialogController? controller = null;
            try
            {
                var dialog = new ExportWindow(defaultName, ExportDataType.List);
                if (dialog.ShowDialog() != true)
                    return;

                if (dialog.Export)
                {
                    controller = await ShowProgressAsync("Please wait", "Please wait while export the data...");
                    await ExportHelper.ExportList(data, dialog.FileName, dialog.ExportType);
                }
                else
                {
                    var content = ExportHelper.CreateListContent(data, dialog.ExportType);
                    CopyToClipboard(content);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex);
            }
            finally
            {
                if (controller != null)
                    await controller.CloseAsync();
            }
        }
    }
}
