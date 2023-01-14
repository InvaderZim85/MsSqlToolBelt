using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MsSqlToolBelt.DataObjects.ClassGen;

namespace MsSqlToolBelt.Ui.ViewModel.Windows;

/// <summary>
/// Provides the logic for the <see cref="View.Windows.DataTypeInputWindow"/>
/// </summary>
internal class DataTypeInputWindowViewModel : ViewModelBase, IDataErrorInfo
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
    /// Backing field for <see cref="ExistingEntry"/>
    /// </summary>
    private bool _existingEntry;

    /// <summary>
    /// Gets or sets the value which indicates if the entry is an existing entry
    /// </summary>
    public bool ExistingEntry
    {
        get => _existingEntry;
        private set => SetProperty(ref _existingEntry, value);
    }

    /// <summary>
    /// Backing field for <see cref="SqlType"/>
    /// </summary>
    private string _sqlType = string.Empty;

    /// <summary>
    /// Gets or sets the sql type
    /// </summary>
    public string SqlType
    {
        get => _sqlType;
        set => SetProperty(ref _sqlType, value.ToUpper());
    }

    /// <summary>
    /// Backing field for <see cref="CSharpType"/>
    /// </summary>
    private string _cSharpType = string.Empty;

    /// <summary>
    /// Gets or sets the CSharp type
    /// </summary>
    public string CSharpType
    {
        get => _cSharpType;
        set => SetProperty(ref _cSharpType, value);
    }

    /// <summary>
    /// Backing field for <see cref="CSharpSystemType"/>
    /// </summary>
    private string _cSharpSystemType = string.Empty;

    /// <summary>
    /// Gets or sets the CSharp system type
    /// </summary>
    public string CSharpSystemType
    {
        get => _cSharpSystemType;
        set => SetProperty(ref _cSharpSystemType, value);
    }

    /// <summary>
    /// Backing field for <see cref="IsNullable"/>
    /// </summary>
    private bool _isNullable;

    /// <summary>
    /// Gets or sets the value which indicates if the C# type is nullable
    /// </summary>
    public bool IsNullable
    {
        get => _isNullable;
        set => SetProperty(ref _isNullable, value);
    }

    /// <summary>
    /// The command which occurs when the user hits the ok button
    /// </summary>
    public ICommand OkCommand => new RelayCommand(GetValues);

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