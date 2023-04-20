﻿using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Common;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for LoadTableWindow.xaml
/// </summary>
public partial class LoadTableWindow : MetroWindow
{
    /// <summary>
    /// Contains the selected table
    /// </summary>
    private readonly TableEntry _selectedTable;

    /// <summary>
    /// The name / path of the MS SQL database
    /// </summary>
    private readonly string _dataSource;

    /// <summary>
    /// The name of the database
    /// </summary>
    private readonly string _database;

    /// <summary>
    /// Creates a new instance of the <see cref="LoadTableWindow"/>
    /// </summary>
    /// <param name="dataSource">The name / path of the MS SQL database</param>
    /// <param name="database">The name of the database</param>
    /// <param name="selectedTable">The selected table</param>
    public LoadTableWindow(string dataSource, string database, TableEntry selectedTable)
    {
        InitializeComponent();

        _selectedTable = selectedTable;
        _dataSource = dataSource;
        _database = database;
    }

    /// <summary>
    /// Sets the sql text
    /// </summary>
    /// <param name="sql"></param>
    private void SetSqlText(string sql)
    {
        CodeEditor.Text = sql;
        CodeEditor.ScrollToHome();
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="LoadTableWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void LoadTableWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        CodeEditor.InitAvalonEditor(CodeType.Sql);

        if (DataContext is LoadTableWindowViewModel viewModel)
            viewModel.InitViewModel(_dataSource, _database, _selectedTable, SetSqlText);
    }
}