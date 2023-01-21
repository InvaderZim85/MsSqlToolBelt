using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.DataObjects.Internal;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions which are needed for the filter (ignore list)
/// </summary>
internal static class FilterHelper
{
    /// <summary>
    /// Checks if the value is valid
    /// </summary>
    /// <param name="value">The value which should be checked</param>
    /// <param name="ignoreList">The list with the ignore filters</param>
    /// <returns><see langword="true"/> is the entry is valid, otherwise <see langword="false"/></returns>
    public static bool IsValid(this string value, IEnumerable<FilterEntry> ignoreList)
    {
        return (from entry in ignoreList
            let filterType = (FilterType)entry.FilterTypeId
            select filterType switch
            {
                FilterType.Contains => value.ContainsIgnoreCase(entry.Value),
                FilterType.Equals => value.EqualsIgnoreCase(entry.Value),
                FilterType.StartsWith => value.StartsWith(entry.Value, StringComparison.OrdinalIgnoreCase),
                FilterType.EndsWith => value.EndsWith(entry.Value, StringComparison.OrdinalIgnoreCase),
                _ => false
            }).All(invalid => !invalid);
    }
}