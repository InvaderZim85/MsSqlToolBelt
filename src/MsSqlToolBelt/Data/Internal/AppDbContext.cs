using Microsoft.EntityFrameworkCore;
using MsSqlToolBelt.DataObjects.Internal;
using System.IO;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Data.Internal;

/// <summary>
/// Provides the database context
/// </summary>
/// <param name="disableTracking"><see langword="true"/> to disable the tracking, otherwise <see langword="false" /> (optional)</param>
internal class AppDbContext(bool disableTracking = false) : DbContext
{
    /// <summary>
    /// Contains the connection string
    /// </summary>
    private readonly string _conString = $"Data Source={Path.Combine(Core.GetBaseDirPath(), "MsSqlToolBelt.Settings.db")}";

    /// <summary>
    /// Gets or sets the list with the server
    /// </summary>
    public DbSet<ServerEntry> ServerEntries => Set<ServerEntry>();

    /// <summary>
    /// Gets or sets the list with the filter entries
    /// </summary>
    public DbSet<FilterEntry> FilterEntries => Set<FilterEntry>();

    /// <summary>
    /// Gets or sets the list with the settings
    /// </summary>
    public DbSet<SettingsEntry> Settings => Set<SettingsEntry>();

    /// <summary>
    /// Gets or sets the list with the search history
    /// </summary>
    public DbSet<SearchHistoryEntry> SearchHistory => Set<SearchHistoryEntry>();

    /// <summary>
    /// Occurs when the context is configured
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_conString);

        if (disableTracking)
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    /// <summary>
    /// Occurs when the context models are created
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServerEntry>().HasIndex(x => new
        {
            x.Name,
            x.DefaultDatabase
        }).IsUnique();
        modelBuilder.Entity<SettingsEntry>().HasIndex(x => x.KeyId).IsUnique();
        modelBuilder.Entity<SearchHistoryEntry>().HasIndex(x => x.SearchEntry);
    }
}