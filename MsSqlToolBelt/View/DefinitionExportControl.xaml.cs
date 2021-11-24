using System.Windows.Controls;
using MsSqlToolBelt.ViewModel;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for DefinitionExportControl.xaml
    /// </summary>
    public partial class DefinitionExportControl : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DefinitionExportControl"/>
        /// </summary>
        public DefinitionExportControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the connector of the view model
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public void SetConnector(Connector connector)
        {
            if (DataContext is DefinitionExportControlViewModel viewModel)
                viewModel.SetConnector(connector);
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            if (DataContext is DefinitionExportControlViewModel viewModel)
                viewModel.Clear();
        }

        /// <summary>
        /// Occurs when the text of the info text box was changed
        /// </summary>
        /// <param name="sender">The <see cref="TextBoxInfo"/></param>
        /// <param name="e">The event arguments</param>
        private void TextBoxInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxInfo.ScrollToEnd();
        }
    }
}
