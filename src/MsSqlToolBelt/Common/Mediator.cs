using MsSqlToolBelt.Common.Enums;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides functions to perform actions
/// </summary>
internal static class Mediator
{
    /// <summary>
    /// Contains the list with the actions which should be executed when the specified key is selected
    /// </summary>
    private static readonly SortedList<MediatorKey, Action> Actions = [];

    /// <summary>
    /// Contains the list with the async actions which should be executed when the specified key is selected
    /// </summary>
    private static readonly SortedList<MediatorKey, Func<Task>> AsyncFunctions = [];

    /// <summary>
    /// Adds a new action
    /// </summary>
    /// <param name="key">The key of the action</param>
    /// <param name="action">The action</param>
    public static void AddAction(MediatorKey key, Action action)
    {
        Actions[key] = action;
    }

    /// <summary>
    /// Adds a new async action
    /// </summary>
    /// <param name="key">The key of the function</param>
    /// <param name="action">The action / function which should be executed</param>
    public static void AddAsyncFunction(MediatorKey key, Func<Task> action)
    {
        AsyncFunctions[key] = action;
    }

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <param name="key">The key of the action</param>
    public static void ExecuteAction(MediatorKey key)
    {
        if (!Actions.TryGetValue(key, out var action))
            return;

        // Execute the action
        action();
    }

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <param name="key">The key of the action</param>
    /// <returns>The awaitable task</returns>
    public static Task ExecuteFunctionAsync(MediatorKey key)
    {
        return !AsyncFunctions.TryGetValue(key, out var func) 
            ? Task.CompletedTask 
            : func(); // Execute the action / function
    }

    /// <summary>
    /// Removes all actions
    /// </summary>
    public static void RemoveAll()
    {
        Actions.Clear();
        AsyncFunctions.Clear();
    }

    /// <summary>
    /// Removes an action
    /// </summary>
    /// <param name="key">The key of the action</param>
    public static void Remove(MediatorKey key)
    {
        if (Actions.ContainsKey(key))
            Actions.Remove(key);
        else if (AsyncFunctions.ContainsKey(key))
            AsyncFunctions.Remove(key);
    }
}