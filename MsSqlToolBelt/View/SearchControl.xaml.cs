using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.ViewModel;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SearchControl"/>
        /// </summary>
        public SearchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the connector of the view model
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public void SetConnector(Connector connector)
        {
            if (DataContext is SearchControlViewModel viewModel)
                viewModel.SetConnector(connector);
        }

        /// <summary>
        /// Sets the text of the avalon editor
        /// </summary>
        /// <param name="text">The desired text</param>
        private void SetSqlText(string text)
        {
            _sqlEditor.Text = text;
        }

        /// <summary>
        /// Sets the sql schema of the editor window
        /// </summary>
        private void SetSqlSchema()
        {
            Helper.InitAvalonEditor(_sqlEditor);
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            if (!(DataContext is SearchControlViewModel viewModel))
                return;

            viewModel.InitViewModel(SetSqlText);
            SetSqlSchema();
        }

        /// <summary>
        /// Occurs when the user hits the CTRL + C while the data grid has the focus
        /// </summary>
        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(ResultGrid.SelectedItem is SearchResult result))
                return;

            Clipboard.SetText(result.Name);
        }
    }
}
