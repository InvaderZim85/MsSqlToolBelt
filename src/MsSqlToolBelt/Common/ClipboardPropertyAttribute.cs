namespace MsSqlToolBelt.Common;

/// <summary>
/// The attribute which indicates if the property should be used when a single line of a grid is copied with CTRL + C
/// <para />
/// <b>NOTE</b>: If the property is linked to more than one property, the first occurrence is used!
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class ClipboardPropertyAttribute : Attribute;