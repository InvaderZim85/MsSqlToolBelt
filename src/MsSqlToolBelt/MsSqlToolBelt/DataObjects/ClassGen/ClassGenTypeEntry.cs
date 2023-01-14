using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.ClassGen;

/// <summary>
/// Represents a class gen type
/// </summary>
public class ClassGenTypeEntry : ObservableObject
{
    /// <summary>
    /// Backing field for <see cref="Id"/>
    /// </summary>
    private int _id;

    /// <summary>
    /// Gets or sets the id (hash code of the value, auto generated, only for internal purpose)
    /// </summary>
    [JsonIgnore]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
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
        set => SetProperty(ref _sqlType, value);
    }

    /// <summary>
    /// Backing field for <see cref="CSharpType"/>
    /// </summary>
    private string _cSharpType = string.Empty;

    /// <summary>
    /// Gets or sets the name of the csharp type
    /// </summary>
    [Appearance(Name = "C# Type")]
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
    /// Gets or sets the name of the csharp system type (like <see langword="System.Int32"/> for an <see langword="int"/>)
    /// </summary>
    [Appearance(Name = "C# System Type")]
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
    /// Gets or sets the is nullable value
    /// </summary>
    public bool IsNullable
    {
        get => _isNullable;
        set => SetProperty(ref _isNullable, value);
    }
}