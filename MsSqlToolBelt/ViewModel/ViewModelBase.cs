using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using MsSqlToolBelt.DataObjects;
using Serilog;
using ZimLabs.TableCreator;
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
        private readonly IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Backing field for <see cref="SaveAsTypes"/>
        /// </summary>
        private ObservableCollection<TextValueItem> _saveAsTypes;

        /// <summary>
        /// Gets or sets the list with the save types
        /// </summary>
        public ObservableCollection<TextValueItem> SaveAsTypes
        {
            get => _saveAsTypes;
            private set => SetField(ref _saveAsTypes, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedSaveType"/>
        /// </summary>
        private TextValueItem _selectedSaveType;

        /// <summary>
        /// Gets or sets the selected save type
        /// </summary>
        public TextValueItem SelectedSaveType
        {
            get => _selectedSaveType;
            set => SetField(ref _selectedSaveType, value);
        }

        /// <summary>
        /// Backing field for <see cref="SaveAsTypes"/>
        /// </summary>
        private ObservableCollection<TextValueItem> _copyAsTypes;

        /// <summary>
        /// Gets or sets the list with the save types
        /// </summary>
        public ObservableCollection<TextValueItem> CopyAsTypes
        {
            get => _copyAsTypes;
            private set => SetField(ref _copyAsTypes, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedCopyType"/>
        /// </summary>
        private TextValueItem _selectedCopyType;

        /// <summary>
        /// Gets or sets the selected copy type
        /// </summary>
        public TextValueItem SelectedCopyType
        {
            get => _selectedCopyType;
            set => SetField(ref _selectedCopyType, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ViewModelBase"/>
        /// </summary>
        protected ViewModelBase()
        {
            _dialogCoordinator = DialogCoordinator.Instance;
        }

        /// <summary>
        /// Init the save / copy types
        /// </summary>
        protected void InitSaveCopyTypes()
        {
            SaveAsTypes = new ObservableCollection<TextValueItem>(CustomEnums.GetValueList<OutputType>("Save as"));
            CopyAsTypes = new ObservableCollection<TextValueItem>(CustomEnums.GetValueList<OutputType>("Copy as"));

            SelectedCopyType = CopyAsTypes.FirstOrDefault(f => f.Id == (int) OutputType.Markdown);
            SelectedSaveType = SaveAsTypes.FirstOrDefault(f => f.Id == (int) OutputType.Markdown);
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
            Log.Information("{title} - {message}", title, message);
        }

        /// <summary>
        /// Shows a question dialog with two buttons
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="okButtonText">The text of the ok button (optional)</param>
        /// <param name="cancelButtonText">The text of the cancel button (optional)</param>
        /// <returns>The dialog result</returns>
        protected async Task<MessageDialogResult> ShowQuestion(string title, string message, string okButtonText = "OK",
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
        /// <param name="caller">The caller of the method (auto filled)</param>
        /// <returns>The awaitable task</returns>
        protected async Task ShowError(Exception ex, [CallerMemberName] string caller = "")
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occurred: {ex.Message}");
            Log.Error(ex, "An error has occurred in method '{method}'", caller);
        }

        /// <summary>
        /// Shows a progress dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="setIndeterminate">true to set the controller to indeterminate, otherwise false (optional)</param>
        /// <returns>The progress controller</returns>
        protected async Task<ProgressDialogController> ShowProgress(string title, string message, bool setIndeterminate = true)
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
        protected async Task<string> ShowInput(string title, string message)
        {
            var result = await _dialogCoordinator.ShowInputAsync(this, title, message);

            return result;
        }


        /// <summary>
        /// Copies the values as formatted string into the clipboard
        /// </summary>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <param name="type">The output type</param>
        /// <param name="values">The list with the values</param>
        protected static void CopyValues<T>(OutputType type, IEnumerable<T> values) where T : class
        {
            Clipboard.SetText(values.CreateTable(type));
        }

        /// <summary>
        /// Copies the given content to the clipboard
        /// </summary>
        /// <param name="content">The content which should be copied</param>
        protected static void CopyToClipboard(string content)
        {
            Clipboard.SetText(content);
        }

        /// <summary>
        /// Saves the values as formatted string into a file
        /// </summary>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <param name="type">The output type</param>
        /// <param name="values">The list with the values</param>
        /// <param name="name">The file name</param>
        protected static void SaveValues<T>(OutputType type, IEnumerable<T> values, string name) where T : class
        {
            values.Export(name, type);
        }
    }
}
