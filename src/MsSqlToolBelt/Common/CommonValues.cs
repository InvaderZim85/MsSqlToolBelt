using MsSqlToolBelt.DataObjects.ClassGen;
using System.Collections.ObjectModel;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides several common values
/// </summary>
internal static class CommonValues
{
    /// <summary>
    /// Gets the list with the "replace" values
    /// </summary>
    /// <param name="includeUnderscore"><see langword="true"/> to include the underscore in the list, otherwise <see langword="false"/> (optional)</param>
    /// <returns>The list with the "replace" values</returns>
    public static List<ReplaceEntry> GetReplaceList(bool includeUnderscore = true)
    {
        var tmpList = new List<ReplaceEntry>
        {
            new(" ", ""), // this should never happen...
            new("ä", "ae"),
            new("ö", "oe"),
            new("ü", "ue"),
            new("ß", "ss"),
            new("Ä", "Ae"),
            new("Ö", "Oe"),
            new("Ü", "Ue")
        };

        if (includeUnderscore)
            tmpList.Add(new ReplaceEntry("_", ""));

        return tmpList;
    }

    /// <summary>
    /// Gets the list with the modifier
    /// </summary>
    /// <returns>The list with the modifier</returns>
    public static ObservableCollection<string> GetModifierList()
    {
        return
        [
            "public",
            "internal",
            "protected",
            "protected internal"
        ];
    }
}
