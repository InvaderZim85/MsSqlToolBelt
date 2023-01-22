using System;
using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using MsSqlToolBelt.DataObjects.ClassGen;
using MsSqlToolBelt.Ui.ViewModel.Windows;

namespace MsSqlToolBelt.Ui.View.Windows;

/// <summary>
/// Interaction logic for DataTypeInputWindow.xaml
/// </summary>
public partial class DataTypeInputWindow : MetroWindow
{
    /// <summary>
    /// The list with the data types
    /// </summary>
    private readonly List<ClassGenTypeEntry> _typeList;

    /// <summary>
    /// Gets the entry
    /// </summary>
    public ClassGenTypeEntry Entry { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="DataTypeInputWindow"/>
    /// </summary>
    /// <param name="typeList">The list with the types</param>
    /// <param name="entry">The entry which should be edited</param>
    public DataTypeInputWindow(List<ClassGenTypeEntry> typeList, ClassGenTypeEntry? entry = null)
    {
        InitializeComponent();

        _typeList = typeList;
        Entry = entry ?? new ClassGenTypeEntry();
    }

    /// <summary>
    /// Occurs when the window was loaded
    /// </summary>
    /// <param name="sender">The <see cref="DataTypeInputWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void DataTypeInputWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DataTypeInputWindowViewModel viewModel)
        {
            viewModel.InitViewModel(_typeList, Entry, SetEntry);
        }
    }

    /// <summary>
    /// Sets the content of the entry
    /// </summary>
    /// <param name="entry">The entry</param>
    private void SetEntry(ClassGenTypeEntry entry)
    {
        Entry = entry;
        DialogResult = true;
    }

    /// <summary>
    /// Occurs when the user hits the cancel button
    /// </summary>
    /// <param name="sender">The cancel button</param>
    /// <param name="e">The event arguments</param>
    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    /// <summary>
    /// Occurs when the window was rendered
    /// </summary>
    /// <param name="sender">The <see cref="DataTypeInputWindow"/></param>
    /// <param name="e">The event arguments</param>
    private void DataTypeInputWindow_OnContentRendered(object? sender, EventArgs e)
    {
        if (DataContext is DataTypeInputWindowViewModel viewModel)
            viewModel.SetValues();
    }
}