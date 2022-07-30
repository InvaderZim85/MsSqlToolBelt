using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data.Internal;
using MsSqlToolBelt.DataObjects.Internal;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the interaction with the search history
/// </summary>
public class SearchHistoryManager
{
    /// <summary>
    /// The app context
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// The instance for the interaction with the settings
    /// </summary>
    private readonly SettingsManager _settingsManager;

    /// <summary>
    /// Gets the list with the search history
    /// </summary>
    public List<SearchHistoryEntry> SearchHistory { get; private set; } = new();

    /// <summary>
    /// Gets or sets the selected entry
    /// </summary>
    public SearchHistoryEntry? SelectedEntry { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="SearchHistoryManager"/>
    /// </summary>
    /// <param name="settingsManager">The instance for the interaction with the settings</param>
    public SearchHistoryManager(SettingsManager settingsManager)
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();

        _settingsManager = settingsManager;
    }

    /// <summary>
    /// Loads the search history and stores them into <see cref="SearchHistory"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadSearchHistoryAsync()
    {
        SearchHistory = await _context.SearchHistory.ToListAsync();
    }

    /// <summary>
    /// Adds a new search entry
    /// </summary>
    /// <param name="entry">The entry which should be added</param>
    /// <returns>The awaitable task</returns>
    public async Task AddSearchEntryAsync(string entry)
    {
        await CheckEntryCountAsync();

        var tmpEntry =
            await _context.SearchHistory.FirstOrDefaultAsync(f => f.SearchEntry.ToLower().Equals(entry.ToLower()));

        if (tmpEntry == null)
        {
            await _context.SearchHistory.AddAsync(new SearchHistoryEntry(entry));
        }
        else
        {
            tmpEntry.DateTime = DateTime.Now;
            _context.SearchHistory.Update(tmpEntry);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks the entry count and removes an entry if needed
    /// </summary>
    /// <returns>The awaitable task</returns>
    private async Task CheckEntryCountAsync()
    {
        var entryCount = await _settingsManager.LoadSettingsValueAsync(SettingsKey.SearchHistoryEntryCount, 10);
        var historyCount = await _context.SearchHistory.AsNoTracking().CountAsync();
        if (historyCount < entryCount)
            return;

        var deleteCount = historyCount - entryCount + 1;

        for (var i = 0; i < deleteCount; i++)
        {
            // Get the last entry
            var lastEntry = await _context.SearchHistory.OrderBy(o => o.DateTime).FirstOrDefaultAsync();
            if (lastEntry == null)
                continue;

            _context.SearchHistory.Remove(lastEntry);

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes the <see cref="SelectedEntry"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task DeleteEntryAsync()
    {
        if (SelectedEntry == null)
            return;

        _context.SearchHistory.Remove(SelectedEntry);
        SearchHistory.Remove(SelectedEntry);
        SelectedEntry = null;

        await _context.SaveChangesAsync();
    }
}