using Microsoft.EntityFrameworkCore;
using MsSqlToolBelt.DataObjects.Internal;
using System.IO;
using ZimLabs.CoreLib;

namespace MsSqlToolBelt.Data.Internal;

/// <summary>
/// Provides the database context
/// </summary>
internal class AppDbContext : DbContext
{
    /// <summary>
    /// Contains the connection string
    /// </summary>
    private readonly string _conString;

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
    /// Creates a new instance of the <see cref="AppDbContext"/>
    /// </summary>
    public AppDbContext()
    {
        _conString = $"Data Source={Path.Combine(Core.GetBaseDirPath(), "MsSqlToolBelt.Settings.db")}";
        
    }

    /// <summary>
    /// Occurs when the context is configured
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_conString);
    }

    /// <summary>
    /// Occurs when the context models are created
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServerEntry>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<SettingsEntry>().HasIndex(x => x.KeyId).IsUnique();
        modelBuilder.Entity<SearchHistoryEntry>().HasIndex(x => x.SearchEntry);
    }
}