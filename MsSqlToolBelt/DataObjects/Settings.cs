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
        public int ServerListCount { get; set; }

        /// <summary>
        /// Gets or sets the list with tables that should be ignored by the class generator
        /// </summary>
        public List<string> TableIgnoreList { get; set; }
    }
}
