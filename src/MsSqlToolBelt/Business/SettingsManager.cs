using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data.Internal;
using MsSqlToolBelt.DataObjects.Internal;
using Serilog;

namespace MsSqlToolBelt.Business;

/// <summary>
/// The instance for the interaction with the settings
/// </summary>
public class SettingsManager
{
    /// <summary>
    /// The app context
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// Gets the list with the servers
    /// </summary>
    public List<ServerEntry> ServerList { get; private set; } = new();

    /// <summary>
    /// Gets the list with the filter entries
    /// </summary>
    public List<FilterEntry> FilterList { get; private set; } = new();

    /// <summary>
    /// Creates a new instance of the <see cref="SettingsManager"/>
    /// </summary>
    public SettingsManager()
    {
        _context = new AppDbContext();
        _context.Database.Migrate();
    }

    #region Settings
    /// <summary>
    /// Loads the specified settings value
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The desired key</param>
    /// <param name="fallback">The desired fallback</param>
    /// <returns>The value</returns>
    public async Task<T> LoadSettingsValueAsync<T>(SettingsKey key, T? fallback = default)
    {
        var value = await _context.Settings.FirstOrDefaultAsync(f => f.KeyId == (int) key);
        if (value == null)
            return fallback == null ? Activator.CreateInstance<T>() : fallback;

        try
        {
            return (T) Convert.ChangeType(value.Value, typeof(T));
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
    public async Task SaveSettingsValueAsync(SettingsKey key, object value)
    {
        var tmpValue = await _context.Settings.FirstOrDefaultAsync(f => f.KeyId == (int) key);
        if (tmpValue != null)
        {
            tmpValue.Value = value.ToString() ?? "";
        }
        else
        {
            await _context.Settings.AddAsync(new SettingsEntry
            {
                KeyId = (int) key,
                Value = value.ToString() ?? ""
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Saves a list of settings
    /// </summary>
    /// <param name="values">The list with the data</param>
    /// <returns>The awaitable task</returns>
    public async Task SaveSettingsValuesAsync(SortedList<SettingsKey, object> values)
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
    /// <param name="withTracking">true to active the tracking, otherwise false (optional)</param>
    /// <returns>The list with the servers</returns>
    public async Task LoadServerAsync(bool withTracking = false)
    {
        ServerList = withTracking
            ? await _context.ServerEntries.ToListAsync()
            : await _context.ServerEntries.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Adds a new server
    /// </summary>
    /// <param name="server">The server which should be added</param>
    /// <returns>The awaitable task</returns>
    public async Task AddServerAsync(ServerEntry server)
    {
        // Check if the entry already exists
        if (_context.ServerEntries.Any(a => a.Name.Equals(server.Name)))
            return;

        var newOrder = (_context.ServerEntries.Any() ? await _context.ServerEntries.MaxAsync(m => m.Order) : 0) + 1;
        server.Order = newOrder;

        await _context.ServerEntries.AddAsync(server);

        await _context.SaveChangesAsync();

        ServerList.Add(server);
    }

    /// <summary>
    /// Updates the specified server
    /// </summary>
    /// <param name="server">The server which should be updated</param>
    /// <returns>The awaitable task</returns>
    public async Task UpdateServerAsync(ServerEntry server)
    {
        _context.ServerEntries.Update(server);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the specified server
    /// </summary>
    /// <param name="server">The server which should be deleted</param>
    /// <returns>The awaitable task</returns>
    public async Task DeleteServerAsync(ServerEntry server)
    {
        ServerList.Remove(server);

        _context.ServerEntries.Remove(server);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Changes the order of the server
    /// </summary>
    /// <param name="server">The server which should be moved</param>
    /// <param name="moveUp">true to move the server "up", otherwise false</param>
    /// <returns>The awaitable task</returns>
    public async Task MoveServerOrderAsync(ServerEntry server, bool moveUp)
    {
        if (server.Order == 1 && moveUp)
            return; // The server is already at the top

        var maxOrder = await _context.ServerEntries.MaxAsync(m => m.Order);
        if (server.Order == maxOrder && !moveUp)
            return; // The server is already at the bottom

        var newOrder = moveUp ? server.Order - 1 : server.Order + 1; // Get the new order
        var otherServer = await _context.ServerEntries.FirstOrDefaultAsync(f => f.Order == newOrder);
        if (otherServer == null)
            return; // Can't find any server, something went wrong

        otherServer.Order = server.Order;
        server.Order = newOrder;

        _context.ServerEntries.UpdateRange(server, otherServer);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region Filter
    /// <summary>
    /// Loads the list with the filters and stores them into <see cref="FilterList"/>
    /// </summary>
    /// <param name="withTracking">true to active the tracking, otherwise false (optional)</param>
    /// <returns>The list with the filter</returns>
    public async Task LoadFilterAsync(bool withTracking = false)
    {
        FilterList =  withTracking
            ? await _context.FilterEntries.ToListAsync()
            : await _context.FilterEntries.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Adds a new filter
    /// </summary>
    /// <param name="filter">The filter which should be added</param>
    /// <returns>The awaitable task</returns>
    public async Task AddFilterAsync(FilterEntry filter)
    {
        // Check if the entry already exists
        if (await _context.FilterEntries.AnyAsync(a => a.FilterTypeId == filter.FilterTypeId &&
                                                       a.Value.ToLower().Equals(filter.Value.ToLower())))
            return;

        await _context.FilterEntries.AddAsync(filter);

        await _context.SaveChangesAsync();

        FilterList.Add(filter);
    }

    /// <summary>
    /// Deletes the specified filter
    /// </summary>
    /// <param name="filter">The filter which should be deleted</param>
    /// <returns>The awaitable task</returns>
    public async Task DeleteFilterAsync(FilterEntry filter)
    {
        // Remove the filter
        FilterList.Remove(filter);

        _context.FilterEntries.Remove(filter);

        await _context.SaveChangesAsync();
    }
    #endregion
}