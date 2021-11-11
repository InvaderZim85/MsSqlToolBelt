namespace MsSqlToolBelt.DataObjects.ClassGenerator
{
    /// <summary>
    /// Represents the settings for the class generator
    /// </summary>
    internal sealed class ClassGenSettingsDto
    {
        /// <summary>
        /// Gets or sets the table
        /// </summary>
        public Table Table { get; set; }

        /// <summary>
        /// Gets or sets the modifier
        /// </summary>
        public string Modifier { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the class should be marked as sealed
        /// </summary>
        public bool MarkAsSealed { get; set; }

        /// <summary>
        /// Gets or sets the name of the class
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if a backing field should be created
        /// </summary>
        public bool BackingField { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if a entity framework class should be created
        /// </summary>
        public bool EfClass { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if a summary should be added
        /// </summary>
        public bool AddSummary { get; set; }

        /// <summary>
        /// Gets or sets the SQL query on the basis of which the class is to be generated
        /// </summary>
        public string SqlQuery { get; set; }
    }
}
