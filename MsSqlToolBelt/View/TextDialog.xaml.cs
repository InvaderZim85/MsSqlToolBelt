using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.Types;
using MsSqlToolBelt.ViewModel;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for TextDialog.xaml
    /// </summary>
    public partial class TextDialog : MetroWindow
    {
        /// <summary>
        /// Contains the caption
        /// </summary>
        private readonly string _caption;

        /// <summary>
        /// The code which should be shown
        /// </summary>
        private readonly string _code;

        /// <summary>
        /// Creates a new instance of the <see cref="TextDialog"/>
        /// </summary>
        /// <param name="title">The title of the window</param>
        /// <param name="caption">The caption which should be shown above the code editor</param>
        /// <param name="code">The code which should be shown</param>
        public TextDialog(string title, string caption, string code)
        {
            InitializeComponent();

            _caption = caption;
            Title = title;
            _code = code;
        }

        /// <summary>
        /// Gets the text of the editor
        /// </summary>
        /// <returns>The text which should be returned</returns>
        private string GetEditorText()
        {
            return CodeEditor.Text;
        }

        /// <summary>
        /// Sets the schema of the editor controls
        /// </summary>
        private void SetSchema()
        {
            // CSharp editor
            Helper.InitAvalonEditor(CodeEditor, CodeType.CSharp);
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        /// <param name="sender">The <see cref="TextDialog"/></param>
        /// <param name="e">The event arguments</param>
        private void TextDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TextDialogViewModel viewModel)
                viewModel.InitViewModel(_caption, GetEditorText);

            CodeEditor.Text = _code;

            SetSchema();
        }

        /// <summary>
        /// Occurs when the user hits the close button
        /// </summary>
        /// <param name="sender">The close button</param>
        /// <param name="e">The event arguments</param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
