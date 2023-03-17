using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.DataObjects.ClassGen;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.DataTypeInputWindow"/>
/// </summary>
internal partial class DataTypeInputWindowViewModel : ViewModelBase, IDataErrorInfo
{
    /// <inheritdoc />
    public string Error => string.Empty;

    /// <inheritdoc />
    public string this[string columnName] => Validate(columnName);

    /// <summary>
    /// The function to set the entry
    /// </summary>
    private Action<ClassGenTypeEntry>? _setEntry;

    /// <summary>
    /// The list with all types (needed for the validation)
    /// </summary>
    private List<ClassGenTypeEntry> _typeList = new();

    /// <summary>
    /// The entry which should be changed
    /// </summary>
    private ClassGenTypeEntry _entry = new();

    /// <summary>
    /// The value which indicates if the entry is an existing entry
    /// </summary>
    [ObservableProperty]
    private bool _existingEntry;

    /// <summary>
    /// The sql type
    /// </summary>
    [ObservableProperty]
    private string _sqlType = string.Empty;

    /// <summary>
    /// The CSharp type
    /// </summary>
    [ObservableProperty]
    private string _cSharpType = string.Empty;

    /// <summary>
    /// The CSharp system type
    /// </summary>
    [ObservableProperty]
    private string _cSharpSystemType = string.Empty;

    /// <summary>
    /// The value which indicates if the C# type is nullable
    /// </summary>
    [ObservableProperty]
    private bool _isNullable;

    /// <summary>
    /// Init the view model
    /// </summary>
    /// <param name="typeList">The list with all existing types</param>
    /// <param name="entry">The entry</param>
    /// <param name="setEntry">The function to set the entry</param>
    public void InitViewModel(List<ClassGenTypeEntry> typeList, ClassGenTypeEntry entry, Action<ClassGenTypeEntry> setEntry)
    {
        _typeList = typeList;
        _entry = entry;
        _setEntry = setEntry;
    }

    /// <summary>
    /// Sets the values
    /// </summary>
    public void SetValues()
    {
        ExistingEntry = _entry.Id != 0;
        SqlType = _entry.SqlType;
        CSharpType = _entry.CSharpType;
        CSharpSystemType = _entry.CSharpSystemType;
        IsNullable = _entry.IsNullable;
    }

    /// <summary>
    /// Validates the specific property
    /// </summary>
    /// <param name="propertyName">The property which should be validated</param>
    /// <returns>The result message</returns>
    private string Validate(string propertyName)
    {
        var result = string.Empty;

        HasErrors = false;
        switch (propertyName)
        {
            case nameof(ClassGenTypeEntry.CSharpType):
                if (string.IsNullOrWhiteSpace(CSharpType))
                {
                    result = "Must not be empty";
                    HasErrors = true;
                }
                break;
            case nameof(ClassGenTypeEntry.CSharpSystemType):
                if (string.IsNullOrWhiteSpace(CSharpSystemType))
                {
                    result = "Must not be empty";
                    HasErrors = true;
                }
                break;
            case nameof(ClassGenTypeEntry.SqlType):
                if (string.IsNullOrWhiteSpace(SqlType))
                {
                    result = "Must not be empty";
                    HasErrors = true;
                }
                else if (_typeList.Where(w => w.Id != _entry.Id).Select(s => s.SqlType).Contains(SqlType.ToUpper()))
                {
                    result = "The SQL type already exists.";
                    HasErrors = true;
                }
                break;
        }

        return result;
    }

    /// <summary>
    /// Sets the values
    /// </summary>
    [RelayCommand]
    private void GetValues()
    {
        if (HasErrors)
            return;

        _entry.SqlType = SqlType.ToUpper();
        _entry.CSharpType = CSharpType;
        _entry.CSharpSystemType = CSharpSystemType;
        _entry.IsNullable = IsNullable;

        _setEntry?.Invoke(_entry);
    }
}