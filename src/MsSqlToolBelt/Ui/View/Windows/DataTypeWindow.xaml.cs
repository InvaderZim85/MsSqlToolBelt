﻿using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.Common;
using MsSqlToolBelt.Ui.ViewModel.Windows;
using System.Windows;
using System.Windows.Input;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for DataTypeWindow.xaml
/// </summary>
public partial class DataTypeWindow : MetroWindow
{
    /// <summary>
    /// Creates a new instance of the <see cref="DataTypeWindow"/>
    /// </summary>
    public DataTypeWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="DataTypeWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void DataTypeWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DataTypeWindowViewModel viewModel) 
            return;

        viewModel.LoadData();
    }

    /// <summary>
    /// Occurs when the user hits the close button
    /// </summary>
    /// <param name="sender">The close button</param>
    /// <param name="e">The event arguments</param>
    private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Occurs when the user hits the CTRL + C
    /// </summary>
    /// <param name="sender">The <see cref="DataGridTypes"/></param>
    /// <param name="e">The event arguments</param>
    private void DataGridTypes_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DataGridTypes.CopyToClipboard<ClassGenTypeEntry>();
    }
}