﻿using System.Windows.Controls;
using MsSqlToolBelt.ViewModel;
using ZimLabs.Database.MsSql;

namespace MsSqlToolBelt.View
{
    /// <summary>
    /// Interaction logic for TableTypeControl.xaml
    /// </summary>
    public partial class TableTypeControl : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TableTypeControl"/>
        /// </summary>
        public TableTypeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the connector of the view model
        /// </summary>
        /// <param name="connector">The instance of the connector</param>
        public void SetConnector(Connector connector)
        {
            if (DataContext is TableTypeControlViewModel viewModel)
                viewModel.SetConnector(connector);
        }
    }
}
