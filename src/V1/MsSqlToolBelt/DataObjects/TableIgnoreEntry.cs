namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents a ignore entry for a table
    /// </summary>
    internal sealed class TableIgnoreEntry
    {
        /// <summary>
        /// Gets or sets the search type
        /// </summary>
        public CustomEnums.FilterType FilterType { get; set; }

        /// <summary>
        /// Gets or sets the value which should be ignored
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the name of the search type
        /// </summary>
        public string FilterTypeView
        {
            get
            {
                var attribute = FilterType.GetAttribute<DescriptionValueAttribute>();
                return attribute == null ? FilterType.ToString() : attribute.ValueText;
            }
        }

        /// <summary>
        /// Returns the filter type with the text
        /// </summary>
        /// <returns>The filter type with the text</returns>
        public override string ToString()
        {
            return $"{FilterTypeView} {Value}";
        }
    }
}
