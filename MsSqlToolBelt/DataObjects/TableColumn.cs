using ZimLabs.TableCreator;
using ZimLabs.WpfBase;

namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents a table column
    /// </summary>
    internal sealed class TableColumn : ObservableObject
    {
        /// <summary>
        /// Gets or sets the ordinal column position
        /// </summary>
        [Appearance(Name = "Column pos.")]
        public int ColumnPosition { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        [Appearance(Ignore = true)]
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the name of the column
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Gets or sets the data type of the column
        /// </summary>
        [Appearance(Name = "Type")]
        public string DataType { get; set; }

        /// <summary>
        /// Backing field for <see cref="Use"/>
        /// </summary>
        private bool _use = true;

        /// <summary>
        /// Gets or sets the value which indicates if the column should be used by the class generator
        /// </summary>
        [Appearance(Ignore = true)]
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
        [Appearance(Ignore = true)]
        public string Alias
        {
            get => _alias;
            set => SetField(ref _alias, value);
        }

        /// <summary>
        /// Gets or sets the value which indicates if the column is nullable
        /// </summary>
        [Appearance(Ignore = true)]
        public string Nullable { get; set; } = "";

        /// <summary>
        /// Gets the value which indicates if the column is nullable
        /// </summary>
        [Appearance(Name = "Nullable")]
        public bool NullableView => Nullable.Equals("YES");

        /// <summary>
        /// Gets or sets the max length of a char column
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the precision of the column (only for numeric / date time columns)
        /// </summary>
        [Appearance(Ignore = true)]
        public int Precision { get; set; }

        /// <summary>
        /// Gets or sets the value for the decimal places (only for values with decimal places)
        /// </summary>
        [Appearance(Ignore = true)]
        public int DecimalPlaceValue { get; set; }

        /// <summary>
        /// Gets the precision value for the view
        /// </summary>
        [Appearance(Name = "Precision")]
        public string PrecisionView => $"{Precision}, {DecimalPlaceValue}";

        /// <summary>
        /// Gets or sets the value which indicates if the column is a part of / or is the primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }
    }
}
