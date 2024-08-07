﻿using CommunityToolkit.Mvvm.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.View.Windows;
using Serilog;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using Timer = System.Timers.Timer;

namespace MsSqlToolBelt.Ui.ViewModel;

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
    /// Gets or sets the value which indicates if there is any error (only needed for the input validation)
    /// </summary>
    protected bool HasErrors { get; set; }

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
        private set => SetProperty(ref _infoMessage, value);
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
    /// Gets the main window
    /// </summary>
    /// <returns>The main window</returns>
    protected static Window GetMainWindow()
    {
        return Application.Current.MainWindow!;
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
    /// Shows a message dialog
    /// </summary>
    /// <param name="entry">The message (header and message)</param>
    /// <returns>The awaitable task</returns>
    protected async Task ShowMessageAsync(MessageEntry entry)
    {
        await _dialogCoordinator.ShowMessageAsync(this, entry.Header, entry.Message);
    }

    /// <summary>
    /// Shows a message dialog where the user can accept or decline an option
    /// </summary>
    /// <param name="entry">The message (header and message)</param>
    /// <returns>The dialog result</returns>
    protected async Task<MessageDialogResult> ShowMessageWithOptionAsync(MessageEntry entry)
    {
        Helper.SetTaskbarPause(true);

        var result = await _dialogCoordinator.ShowMessageAsync(this, entry.Header, entry.Message,
            MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No"
            });

        Helper.SetTaskbarPause(false);

        return result;
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
        Helper.SetTaskbarPause(true);

        var result = await _dialogCoordinator.ShowMessageAsync(this, title, message,
            MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
            {
                AffirmativeButtonText = okButtonText,
                NegativeButtonText = cancelButtonText
            });

        Helper.SetTaskbarPause(false);

        return result;
    }

    /// <summary>
    /// Shows a question dialog with two buttons
    /// </summary>
    /// <param name="entry">The message (header and message)</param>
    /// <param name="okButtonText">The text of the ok button (optional)</param>
    /// <param name="cancelButtonText">The text of the cancel button (optional)</param>
    /// <returns>The dialog result</returns>
    protected Task<MessageDialogResult> ShowQuestionAsync(MessageEntry entry, string okButtonText = "OK",
        string cancelButtonText = "Cancel")
    {
        return ShowQuestionAsync(entry.Header, entry.Message, okButtonText, cancelButtonText);
    }

    /// <summary>
    /// Shows an error message and logs the exception
    /// </summary>
    /// <param name="ex">The exception which was thrown</param>
    /// <param name="messageType">The desired message type</param>
    /// <param name="caller">The name of the method, which calls this method. Value will be filled automatically</param>
    /// <returns>The awaitable task</returns>
    protected async Task ShowErrorAsync(Exception ex, ErrorMessageType messageType,
        [CallerMemberName] string caller = "")
    {
        Helper.SetTaskbarError(true);

        LogError(ex, caller);
        var message = MessageHelper.GetErrorMessage(messageType, ex.Message);
        await _dialogCoordinator.ShowMessageAsync(this, message.Header, message.Message);

        Helper.SetTaskbarError(false);
    }

    /// <summary>
    /// Logs an error
    /// </summary>
    /// <param name="ex">The exception which was thrown</param>
    /// <param name="caller">The name of the method, which calls this method. Value will be filled automatically</param>
    protected static void LogError(Exception ex, [CallerMemberName] string caller = "")
    {
        Log.Error(ex, "An error has occurred. Caller: {caller}", caller);
    }

    /// <summary>
    /// Shows a progress dialog
    /// </summary>
    /// <param name="title">The title of the dialog</param>
    /// <param name="message">The message of the dialog</param>
    /// <param name="ctSource">The cancellation token source (optional)</param>
    /// <returns>The dialog controller</returns>
    protected async Task<ProgressDialogController> ShowProgressAsync(string title, string message, CancellationTokenSource? ctSource = default)
    {
        Helper.SetTaskbarIndeterminate(true);

        var controller = await _dialogCoordinator.ShowProgressAsync(this, title, message, ctSource != null);
        controller.SetIndeterminate();

        if (ctSource != null)
        {
            controller.Canceled += (_, _) => ctSource.Cancel();
        }

        controller.Closed += (_, _) =>
        {
            // Reset the taskbar when the controller was closed
            Helper.SetTaskbarIndeterminate(false);
        };

        return controller;
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
    protected static void CopyToClipboard(string content)
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
                Helper.SetTaskbarIndeterminate(true);

                controller =
                    await _dialogCoordinator.ShowProgressAsync(this, "Please wait",
                        "Please wait while export the data...");
                controller.SetIndeterminate();

                await ExportHelper.ExportObjectAsync(data, dialog.FileName, dialog.ExportType);
            }
            else
            {
                var content = ExportHelper.CreateObjectContent(data, dialog.ExportType);
                CopyToClipboard(content);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            if (controller != null)
            {
                await controller.CloseAsync();
                Helper.SetTaskbarIndeterminate(false);
            }
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
            var dialog = new ExportWindow(defaultName, ExportDataType.List) {Owner = GetMainWindow() };
            if (dialog.ShowDialog() != true)
                return;

            if (dialog.Export)
            {
                Helper.SetTaskbarIndeterminate(true);

                controller =
                    await _dialogCoordinator.ShowProgressAsync(this, "Please wait",
                        "Please wait while export the data...");
                controller.SetIndeterminate();

                await ExportHelper.ExportListAsync(data, dialog.FileName, dialog.ExportType);
            }
            else
            {
                var content = ExportHelper.CreateListContent(data, dialog.ExportType);
                CopyToClipboard(content);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            if (controller != null)
            {
                await controller.CloseAsync();
                Helper.SetTaskbarIndeterminate(false);
            }
        }
    }

    /// <summary>
    /// Exports the desired data table
    /// </summary>
    /// <param name="table">The table with the data</param>
    /// <param name="defaultName">The default name of the file</param>
    /// <returns>The awaitable task</returns>
    protected async Task ExportDataTableAsync(DataTable table, string defaultName)
    {
        ProgressDialogController? controller = null;
        try
        {
            var dialog = new ExportWindow(defaultName, ExportDataType.List) { Owner = GetMainWindow() };
            if (dialog.ShowDialog() != true)
                return;

            if (dialog.Export)
            {
                Helper.SetTaskbarIndeterminate(true);

                controller =
                    await _dialogCoordinator.ShowProgressAsync(this, "Please wait",
                        "Please wait while export the data...");
                controller.SetIndeterminate();

                await ExportHelper.ExportDataTableAsync(table, dialog.FileName, dialog.ExportType);
            }
            else
            {
                var content = ExportHelper.CreateDataTableContent(table, dialog.ExportType);
                CopyToClipboard(content);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex, ErrorMessageType.Save);
        }
        finally
        {
            if (controller != null)
            {
                await controller.CloseAsync();
                Helper.SetTaskbarIndeterminate(false);
            }
        }
    }
}