using ZimLabs.WpfBase;

namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents a table column
    /// </summary>
    internal sealed class TableColumn : ObservableObject
    {
        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the name of the column
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Gets or sets the data type of the column
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Backing field for <see cref="Use"/>
        /// </summary>
        private bool _use = true;

        /// <summary>
        /// Gets or sets the value which indicates if the column should be used by the class generator
        /// </summary>
        public bool Use
        {
            get => _use;
            set => SetField(ref _use, value);
        }

        /// <summary>
        /// Backing field for <see cref="Alias"/>
        /// </summary>
        private string _alias = "";

        /// <summary>
        /// Gets or sets the alias of the column
        /// </summary>
        public string Alias
        {
            get => _alias;
            set => SetField(ref _alias, value);
        }
    }
}
