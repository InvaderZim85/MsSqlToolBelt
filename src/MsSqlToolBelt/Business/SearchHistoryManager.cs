using Microsoft.EntityFrameworkCore;
using MsSqlToolBelt.Common;
using MsSqlToolBelt.Common.Enums;
using MsSqlToolBelt.Data.Internal;
using MsSqlToolBelt.DataObjects.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the interaction with the search history
/// </summary>
public class SearchHistoryManager
{
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
    public SearchHistoryManager()
    {
        using var context = new AppDbContext();
        context.Database.Migrate();
    }

    /// <summary>
    /// Loads the search history and stores them into <see cref="SearchHistory"/>
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task LoadSearchHistoryAsync()
    {
        await using var context = new AppDbContext();
        SearchHistory = await context.SearchHistory.ToListAsync();
    }

    /// <summary>
    /// Adds a new search entry
    /// </summary>
    /// <param name="entry">The entry which should be added</param>
    /// <returns>The awaitable task</returns>
    public static async Task AddSearchEntryAsync(string entry)
    {
        await using var context = new AppDbContext();

        await CheckEntryCountAsync();

        var tmpEntry =
            await context.SearchHistory.FirstOrDefaultAsync(f => f.SearchEntry.ToLower().Equals(entry.ToLower()));

        if (tmpEntry == null)
        {
            await context.SearchHistory.AddAsync(new SearchHistoryEntry(entry));
        }
        else
        {
            tmpEntry.DateTime = DateTime.Now;
            tmpEntry.SearchCount++; // Update the search count
            context.SearchHistory.Update(tmpEntry);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks the entry count and removes an entry if needed
    /// </summary>
    /// <returns>The awaitable task</returns>
    private static async Task CheckEntryCountAsync()
    {
        await using var context = new AppDbContext();

        var entryCount = await SettingsManager.LoadSettingsValueAsync(SettingsKey.SearchHistoryEntryCount,
            DefaultEntries.SearchHistoryCount);
        if (entryCount == 0) // 0 = infinity
            return;

        var historyCount = await context.SearchHistory.AsNoTracking().CountAsync();
        if (historyCount < entryCount)
            return;

        var deleteCount = historyCount - entryCount + 1;

        for (var i = 0; i < deleteCount; i++)
        {
            // Get the last entry
            var lastEntry = await context.SearchHistory.OrderBy(o => o.DateTime).FirstOrDefaultAsync();
            if (lastEntry == null)
                continue;

            context.SearchHistory.Remove(lastEntry);

            await context.SaveChangesAsync();
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

        await using var context = new AppDbContext();

        context.SearchHistory.Remove(SelectedEntry);
        SearchHistory.Remove(SelectedEntry);
        SelectedEntry = null;

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Clears the complete history
    /// </summary>
    /// <returns>The awaitable task</returns>
    public async Task ClearHistoryAsync()
    {
        await using var context = new AppDbContext();

        context.SearchHistory.RemoveRange(context.SearchHistory);
        await context.SaveChangesAsync();

        SearchHistory.Clear();
        SelectedEntry = null;
    }
}