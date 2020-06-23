using System;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.ViewModel
{
    /// <summary>
    /// Provides the base functions of a view model
    /// </summary>
    internal class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// The instance of the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Creates a new instance of the <see cref="ViewModelBase"/>
        /// </summary>
        protected ViewModelBase()
        {
            _dialogCoordinator = DialogCoordinator.Instance;
        }

        /// <summary>
        /// Shows a message dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <returns>The awaitable task</returns>
        protected async Task ShowMessage(string title, string message)
        {
            await _dialogCoordinator.ShowMessageAsync(this, title, message);
        }

        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="ex">The exception which was thrown</param>
        /// <returns>The awaitable task</returns>
        protected async Task ShowError(Exception ex)
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occured: {ex.Message}");
        }

        /// <summary>
        /// Shows a progress dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <returns>The progress controller</returns>
        protected async Task<ProgressDialogController> ShowProgress(string title, string message)
        {
            return await _dialogCoordinator.ShowProgressAsync(this, title, message);
        }
    }
}
