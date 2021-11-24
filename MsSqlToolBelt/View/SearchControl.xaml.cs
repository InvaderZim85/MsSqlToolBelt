using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Search;
using MsSqlToolBelt.DataObjects.Search;
using MsSqlToolBelt.DataObjects.Types;
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
            SearchPanel.Install(SqlEditor);
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
            SqlEditor.Text = text;
            SqlEditor.ScrollToHome();
        }

        /// <summary>
        /// Sets the sql schema of the editor window
        /// </summary>
        private void SetTheme()
        {
            Helper.InitAvalonEditor(SqlEditor, CodeType.Sql);
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            if (DataContext is not SearchControlViewModel viewModel)
                return;

            viewModel.InitViewModel(SetSqlText);
            SetTheme();

            Helper.AddAction("SetTheme", SetTheme);
        }

        /// <summary>
        /// Occurs when the user hits the CTRL + C while the data grid has the focus
        /// </summary>
        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var items = new List<SearchResult>();

            foreach (var item in ResultGrid.SelectedItems)
            {
                if (item is not SearchResult result)
                    continue;

                items.Add(result);
            }

            if (!items.Any())
                return;

            Clipboard.SetText(string.Join(Environment.NewLine, items.Select(s => s.Name).Distinct().OrderBy(o => o)));
        }

        /// <summary>
        /// Clears the content of the control
        /// </summary>
        public void Clear()
        {
            if (DataContext is SearchControlViewModel viewModel)
                viewModel.Clear();
        }
    }
}
