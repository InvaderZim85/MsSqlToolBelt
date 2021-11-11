using System.Collections.Generic;

namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents the settings
    /// </summary>
    internal sealed class Settings
    {
        /// <summary>
        /// Gets or sets the count of server which should be saved
        /// </summary>
        public int ServerListCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets the list with the server
        /// </summary>
        public List<string> ServerList { get; set; } = new();

        /// <summary>
        /// Gets or sets the list with tables that should be ignored by the class generator
        /// </summary>
        public List<TableIgnoreEntry> TableIgnoreList { get; set; } = new();

        /// <summary>
        /// Gets or sets the name of the theme
        /// </summary>
        public string ThemeColor { get; set; }

        /// <summary>
        /// Gets or sets the name of the theme base color
        /// </summary>
        public string ThemeBaseColor { get; set; }
    }
}
