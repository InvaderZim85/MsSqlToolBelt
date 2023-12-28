namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides functions to perform actions
/// </summary>
internal static class Mediator
{
    /// <summary>
    /// Contains the list with the actions which should be executed when the specified key is selected
    /// </summary>
    private static readonly SortedList<string, Action> Actions = [];

    /// <summary>
    /// Adds a new action
    /// </summary>
    /// <param name="key">The key of the action</param>
    /// <param name="action">The action</param>
    public static void AddAction(string key, Action action)
    {
        Actions[key] = action;
    }

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <param name="key">The key of the action</param>
    public static void ExecuteAction(string key)
    {
        if (!Actions.TryGetValue(key, out var value))
            return;

        value();
    }

    /// <summary>
    /// Removes all actions
    /// </summary>
    public static void RemoveAllActions()
    {
        Actions.Clear();
    }

    /// <summary>
    /// Removes an action
    /// </summary>
    /// <param name="key">The key of the action</param>
    public static void RemoveAction(string key)
    {
        if (!Actions.ContainsKey(key))
            return;

        Actions.Remove(key);
    }
}