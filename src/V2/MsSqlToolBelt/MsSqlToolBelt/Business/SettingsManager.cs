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
internal class SettingsManager
{
    /// <summary>
    /// The app context
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// Creates a new instance of the <see cref="SettingsManager"/>
    /// </summary>
    public SettingsManager()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
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
            return (T) Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while loading settings value. Key {key}", key);
            return fallback == null ? Activator.CreateInstance<T>() : fallback;
        }
    }
    #endregion

    #region Server
    /// <summary>
    /// Loads the server list
    /// </summary>
    /// <param name="withTracking">true to active the tracking, otherwise false (optional)</param>
    /// <returns>The list with the servers</returns>
    public async Task<List<ServerEntry>> LoadServerAsync(bool withTracking = false)
    {
        return withTracking
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
    /// Loads the list with the filters
    /// </summary>
    /// <param name="withTracking">true to active the tracking, otherwise false (optional)</param>
    /// <returns>The list with the filter</returns>
    public async Task<List<FilterEntry>> LoadFilterAsync(bool withTracking = false)
    {
        return withTracking
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
        if (_context.FilterEntries.Any(a =>
                a.FilterTypeId == filter.FilterTypeId &&
                a.Value.Equals(filter.Value, StringComparison.OrdinalIgnoreCase)))
            return; 

        await _context.FilterEntries.AddAsync(filter);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the specified filter
    /// </summary>
    /// <param name="filter">The filter which should be updated</param>
    /// <returns></returns>
    public async Task UpdateFilterAsync(FilterEntry filter)
    {
        _context.FilterEntries.Update(filter);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the specified filter
    /// </summary>
    /// <param name="filter">The filter which should be deleted</param>
    /// <returns>The awaitable task</returns>
    public async Task DeleteFilterAsync(FilterEntry filter)
    {
        _context.FilterEntries.Remove(filter);

        await _context.SaveChangesAsync();
    }
    #endregion
}