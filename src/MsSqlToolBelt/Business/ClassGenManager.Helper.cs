using MsSqlToolBelt.Common;
using MsSqlToolBelt.DataObjects.ClassGen;

namespace MsSqlToolBelt.Business;

/// <inheritdoc cref="ClassGenManager"/>
public sealed partial class ClassGenManager
{

    /// <summary>
    /// Cleans the name and removes "illegal" chars like a underscore in the name
    /// </summary>
    /// <param name="name">The original name</param>
    /// <returns>The cleaned name</returns>
    private static string CleanColumnName(string name)
    {
        var replaceValues = CommonValues.GetReplaceList();

        foreach (var entry in replaceValues.Where(w => name.Contains(w.OldValue)))
        {
            name = name.Replace(entry.OldValue, entry.NewValue);
        }

        // If this happens something is pretty broken
        if (name.Length == 0)
            return "Column";

        // Check if the name starts with a digit, if so, add "Column" to prevent errors
        // because a property / variable hast to start with a "char" and not a number...
        if (name[0].IsNumeric())
            name = $"Column{name}";

        return name;
    }

    /// <summary>
    /// Cleans the name of the namespace and removes spaces
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The cleaned namespace name</returns>
    private static string CleanNamespace(string name)
    {
        const char dot = '.';
        if (!name.Contains(dot))
            return name.FirstChatToUpper().Replace(" ", "");

        var content = name.Split(dot, StringSplitOptions.RemoveEmptyEntries).ToList();
        name = string.Join(dot, content.Select(s => s.FirstChatToUpper()));

        return name.FirstChatToUpper().Replace(" ", "");
    }/// <summary>
    /// Generates a "valid" class name
    /// </summary>
    /// <param name="tableName">The original name of the table</param>
    /// <returns>The generated class name</returns>
    private static string GenerateClassName(string tableName)
    {
        IEnumerable<ReplaceEntry> replaceList;

        // Check if the class name contains a underscore
        if (tableName.Contains('_'))
        {
            replaceList = CommonValues.GetReplaceList(false);

            // Split entry at underscore
            var content = tableName.Split('_', StringSplitOptions.RemoveEmptyEntries);

            // Create a new "class" name
            tableName = content.Aggregate(string.Empty, (current, entry) => current + entry.FirstChatToUpper());
        }
        else
        {
            replaceList = CommonValues.GetReplaceList();
            tableName = tableName.FirstChatToUpper();
        }

        // Remove all "invalid" chars
        foreach (var entry in replaceList.Where(w => tableName.Contains(w.OldValue)))
        {
            tableName = tableName.Replace(entry.OldValue, entry.NewValue);
        }

        return tableName;
    }

}