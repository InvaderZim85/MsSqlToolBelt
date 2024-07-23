using Microsoft.EntityFrameworkCore;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data.Internal;
using MsSqlToolBelt.DataObjects.Internal;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Text;
using MsSqlToolBelt.Common;

namespace MsSqlToolBelt.Business;

/// <summary>
/// The instance for the interaction with the settings
/// </summary>
public class SettingsManager
{
    /// <summary>
    /// Gets the list with the servers
    /// </summary>
    public List<ServerEntry> ServerList { get; private set; } = [];

    /// <summary>
    /// Gets the list with the filter entries
    /// </summary>
    public List<FilterEntry> FilterList { get; private set; } = [];

    #region Settings
    /// <summary>
    /// Loads the specified settings value
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The desired key</param>
    /// <param name="fallback">The desired fallback</param>
    /// <returns>The value</returns>
    public static async Task<T> LoadSettingsValueAsync<T>(SettingsKey key, T fallback)
    {
        await using var context = new AppDbContext(true);
        var value = await context.Settings.FirstOrDefaultAsync(f => f.KeyId == (int) key);
        return value == null ? fallback : value.Value.ChangeType(fallback);
    }

    /// <summary>
    /// Loads the values for the specified entries
    /// </summary>
    /// <param name="values">The list with the values which should be loaded</param>
    /// <returns>The awaitable task</returns>
    public static async Task LoadSettingsValuesAsync(List<SettingsValue> values)
    {
        await using var context = new AppDbContext(true);

        var keys = values.Select(s => (int)s.Key).ToList();

        var settings = await context.Settings.Where(w => keys.Contains(w.KeyId)).ToListAsync();

        // Add the settings to the values
        foreach (var entry in settings)
        {
            var value = values.FirstOrDefault(f => (int)f.Key == entry.KeyId);
            if (value == null) 
                continue;

            value.OriginalValue = entry.Value;
        }
    }

    /// <summary>
    /// Loads the specified settings value
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The desired key</param>
    /// <param name="fallback">The desired fallback</param>
    /// <returns>The value</returns>
    public static T LoadSettingsValue<T>(SettingsKey key, T? fallback = default)
    {
        using var context = new AppDbContext(true);
        var value = context.Settings.FirstOrDefault(f => f.KeyId == (int)key);
        if (value == null)
            return fallback == null ? Activator.CreateInstance<T>() : fallback;

        try
        {
            return (T)Convert.ChangeType(value.Value, typeof(T));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while loading settings value. Key {key}", key);
            return fallback == null ? Activator.CreateInstance<T>() : fallback;
        }
    }

    /// <summary>
    /// Saves a settings value
    /// </summary>
    /// <param name="key">The desired key</param>
    /// <param name="value">The value which should be saved</param>
    /// <returns>The awaitable task</returns>
    public static async Task SaveSettingsValueAsync(SettingsKey key, object value)
    {
        await using var context = new AppDbContext();
        var tmpValue = await context.Settings.FirstOrDefaultAsync(f => f.KeyId == (int) key);
        if (tmpValue != null)
        {
            tmpValue.Value = value.ToString() ?? "";
        }
        else
        {
            await context.Settings.AddAsync(new SettingsEntry
            {
                KeyId = (int) key,
                Value = value.ToString() ?? ""
            });
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Saves a list of settings
    /// </summary>
    /// <param name="values">The list with the data</param>
    /// <returns>The awaitable task</returns>
    public static async Task SaveSettingsValuesAsync(SortedList<SettingsKey, object> values)
    {
        foreach (var value in values)
        {
            await SaveSettingsValueAsync(value.Key, value.Value);
        }
    }
    #endregion

    #region Server
    /// <summary>
    /// Loads the server list and stores them into <see cref="ServerList"/>
    /// </summary>
    /// <param name="withTracking">true to activate the tracking, otherwise false (optional)</param>
    /// <returns>The list with the servers</returns>
    public async Task LoadServerAsync(bool withTracking = false)
    {
        await using var context = new AppDbContext(true);
        ServerList = withTracking
            ? await context.ServerEntries.ToListAsync()
            : await context.ServerEntries.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Adds a new server
    /// </summary>
    /// <param name="server">The server which should be added</param>
    /// <returns>The awaitable task</returns>
    public async Task AddServerAsync(ServerEntry server)
    {
        await using var context = new AppDbContext();

        // Check if the entry already exists
        if (await context.ServerEntries.AnyAsync(a =>
                a.Name.Equals(server.Name) && a.DefaultDatabase.Equals(server.DefaultDatabase)))
            return;

        var newOrder = (await context.ServerEntries.AnyAsync() ? await context.ServerEntries.MaxAsync(m => m.Order) : 0) + 1;
        server.Order = newOrder;

        await context.ServerEntries.AddAsync(server);

        await context.SaveChangesAsync();

        ServerList.Add(server);
    }

    /// <summary>
    /// Updates the specified server
    /// </summary>
    /// <param name="server">The server which should be updated</param>
    /// <returns>The awaitable task</returns>
    public static async Task UpdateServerAsync(ServerEntry server)
    {
        await using var context = new AppDbContext();

        context.ServerEntries.Update(server);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the specified server
    /// </summary>
    /// <param name="server">The server which should be deleted</param>
    /// <returns>The awaitable task</returns>
    public async Task DeleteServerAsync(ServerEntry server)
    {
        await using var context = new AppDbContext();

        ServerList.Remove(server);

        context.ServerEntries.Remove(server);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Changes the order of the server
    /// </summary>
    /// <param name="server">The server which should be moved</param>
    /// <param name="moveUp">true to move the server "up", otherwise false</param>
    /// <returns>The awaitable task</returns>
    public static async Task MoveServerOrderAsync(ServerEntry server, bool moveUp)
    {
        await using var context = new AppDbContext();

        if (server.Order == 1 && moveUp)
            return; // The server is already at the top

        var maxOrder = await context.ServerEntries.MaxAsync(m => m.Order);
        if (server.Order == maxOrder && !moveUp)
            return; // The server is already at the bottom

        var newOrder = moveUp ? server.Order - 1 : server.Order + 1; // Get the new order
        var otherServer = await context.ServerEntries.FirstOrDefaultAsync(f => f.Order == newOrder);
        if (otherServer == null)
            return; // Can't find any server, something went wrong

        otherServer.Order = server.Order;
        server.Order = newOrder;

        context.ServerEntries.UpdateRange(server, otherServer);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if the server entry is unique (name and database will be checked)
    /// </summary>
    /// <param name="server">The name / path of the server</param>
    /// <param name="database">The name of the database</param>
    /// <returns><see langword="true"/> when the entry is unique, otherwise <see langword="false"/></returns>
    public static async Task<bool> ServerDatabaseUniqueAsync(string server, string database)
    {
        await using var context = new AppDbContext();

        return !await context.ServerEntries.AsNoTracking().AnyAsync(a => a.Name.Equals(server) &&
                                                                        a.DefaultDatabase.Equals(database));
    }
    #endregion

    #region Filter
    /// <summary>
    /// Loads the list with the filters and stores them into <see cref="FilterList"/>
    /// </summary>
    /// <returns>The list with the filter</returns>
    public async Task LoadFilterAsync()
    {
        await using var context = new AppDbContext(true);

        FilterList = await context.FilterEntries.ToListAsync();
    }

    /// <summary>
    /// Adds a new filter
    /// </summary>
    /// <param name="filter">The filter which should be added</param>
    /// <returns>The awaitable task</returns>
    public async Task AddFilterAsync(FilterEntry filter)
    {
        await using var context = new AppDbContext();

        // Check if the entry already exists
        if (await context.FilterEntries.AnyAsync(a => a.FilterTypeId == filter.FilterTypeId &&
                                                       a.Value.ToLower().Equals(filter.Value.ToLower())))
            return;

        await context.FilterEntries.AddAsync(filter);

        await context.SaveChangesAsync();

        FilterList.Add(filter);
    }

    /// <summary>
    /// Deletes the specified filter
    /// </summary>
    /// <param name="filter">The filter which should be deleted</param>
    /// <returns>The awaitable task</returns>
    public async Task DeleteFilterAsync(FilterEntry filter)
    {
        await using var context = new AppDbContext();

        // Remove the filter
        FilterList.Remove(filter);

        context.FilterEntries.Remove(filter);

        await context.SaveChangesAsync();
    }
    #endregion

    #region Export / Import
    /// <summary>
    /// Exports all existing settings
    /// </summary>
    /// <param name="filepath">The path of the destination</param>
    /// <returns>The awaitable task</returns>
    public static async Task ExportSettingsAsync(string filepath)
    {
        await using var context = new AppDbContext(true);

        var settings = await context.Settings.ToListAsync();
        var server = await context.ServerEntries.ToListAsync();
        var filter = await context.FilterEntries.ToListAsync();

        var data = new SettingsDto
        {
            Settings = settings,
            Servers = server,
            Filters = filter
        };

        var content = JsonConvert.SerializeObject(data, Formatting.Indented);

        await File.WriteAllTextAsync(filepath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Imports the settings
    /// </summary>
    /// <param name="filepath">The path of the settings file</param>
    /// <param name="overrideSettings"><see langword="true"/> to override all existing settings with the new one, otherwise <see langword="false"/></param>
    /// <returns>The awaitable task</returns>
    public static async Task ImportSettingsAsync(string filepath, bool overrideSettings)
    {
        if (!File.Exists(filepath)) 
            return;

        var content = await File.ReadAllTextAsync(filepath);

        if (string.IsNullOrWhiteSpace(content))
            return;

        var data = JsonConvert.DeserializeObject<SettingsDto>(content) ?? new SettingsDto();

        // Check if the dto is default
        if (data.Servers.Count == 0 && data.Settings.Count == 0 && data.Filters.Count == 0)
            return;

        // Import settings
        await ImportSettingsAsync(data.Settings, overrideSettings);

        // Import server
        await ImportServerAsync(data.Servers, overrideSettings);

        // Import filter
        await ImportFilterAsync(data.Filters, overrideSettings);
    }

    /// <summary>
    /// Imports the server entries
    /// </summary>
    /// <param name="server">The list with the server</param>
    /// <param name="overrideSettings"><see langword="true"/> to override all existing settings with the new one, otherwise <see langword="false"/></param>
    /// <returns>The awaitable task</returns>
    private static async Task ImportServerAsync(List<ServerEntry> server, bool overrideSettings)
    {
        await using var context = new AppDbContext();

        // Check if there is already an entry
        if (await context.ServerEntries.AsNoTracking().AnyAsync())
        {
            var currentOrder = await context.ServerEntries.AsNoTracking().MaxAsync(m => m.Order);
            foreach (var entry in server)
            {
                var existingEntry = await context.ServerEntries.FirstOrDefaultAsync(f => f.Name.ToLower().Equals(entry.Name.ToLower()));
                if (existingEntry != null && overrideSettings)
                {
                    existingEntry.AutoConnect = entry.AutoConnect;
                    existingEntry.DefaultDatabase = entry.DefaultDatabase;
                    existingEntry.Description = entry.Description;
                }
                else if (existingEntry == null)
                {
                    var insertEntry = new ServerEntry(entry)
                    {
                        Order = ++currentOrder
                    };

                    await context.ServerEntries.AddAsync(insertEntry);
                }
            }
        }
        else
        {
            // Add the new entries
            foreach (var entry in server)
            {
                await context.ServerEntries.AddAsync(new ServerEntry(entry));
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Imports the settings entries
    /// </summary>
    /// <param name="settings">The list with the settings</param>
    /// <param name="overrideSettings"><see langword="true"/> to override all existing settings with the new one, otherwise <see langword="false"/></param>
    /// <returns>The awaitable task</returns>
    private static async Task ImportSettingsAsync(IEnumerable<SettingsEntry> settings, bool overrideSettings)
    {
        await using var context = new AppDbContext();

        foreach (var entry in settings.Where(entry => entry.KeyId != (int)SettingsKey.UpTime))
        {
            var existingEntry = await context.Settings.FirstOrDefaultAsync(f => f.KeyId == entry.KeyId);
            if (existingEntry != null && overrideSettings)
            {
                existingEntry.Value = entry.Value;
            }
            else if (existingEntry == null)
            {
                var insertEntry = new SettingsEntry(entry);
                await context.Settings.AddAsync(insertEntry);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Imports the filter entries
    /// </summary>
    /// <param name="filter">The list with the filters</param>
    /// <param name="overrideSettings"><see langword="true"/> to override all existing settings with the new one, otherwise <see langword="false"/></param>
    /// <returns>The awaitable task</returns>
    private static async Task ImportFilterAsync(List<FilterEntry> filter, bool overrideSettings)
    {
        await using var context = new AppDbContext();

        foreach (var entry in filter)
        {
            var existingEntry =
                await context.FilterEntries.FirstOrDefaultAsync(f => f.Value.ToLower().Equals(entry.Value.ToLower()));
            if (existingEntry != null && overrideSettings)
            {
                existingEntry.FilterTypeId = entry.FilterTypeId;
            }
            else if (existingEntry == null)
            {
                var insertEntry = new FilterEntry(entry);
                await context.FilterEntries.AddAsync(insertEntry);
            }
        }

        await context.SaveChangesAsync();
    }
    #endregion

    #region Various
    /// <summary>
    /// Loads the tab settings
    /// </summary>
    /// <returns>The list with the tab settings</returns>
    public static SortedList<TabControlEntry, bool> LoadTabSettings()
    {
        var result = new SortedList<TabControlEntry, bool>();
        // Init the list with the default values
        foreach (var entry in Enum.GetValues<TabControlEntry>())
        {
            result.Add(entry, true);
        }

        var values = LoadSettingsValue(SettingsKey.TabSettings, string.Empty);
        if (string.IsNullOrEmpty(values))
            return result;

        var content = values.Split(';');
        if (content.Length != result.Count)
            return result; // Mismatch

        foreach (var entry in Enum.GetValues<TabControlEntry>())
        {
            var index = (int)entry;
            var value = index > content.Length - 1 || content[index].StringToBool();
            result[entry] = value;
        }

        return result;
    }

    /// <summary>
    /// Saves the tab settings
    /// </summary>
    /// <param name="values">The list with the settings</param>
    /// <returns>The awaitable task</returns>
    public static async Task SaveTabSettingsAsync(SortedList<TabControlEntry, bool> values)
    {
        var tmpValues = values.OrderBy(o => (int)o.Key).Select(s => s.Value.BoolToString()).ToList();
        var tmpContent = string.Join(';', tmpValues);

        await SaveSettingsValueAsync(SettingsKey.TabSettings, tmpContent);

        // Execute the mediator function
        Mediator.ExecuteAction(MediatorKey.SetTabVisibility);
    }
    #endregion
}