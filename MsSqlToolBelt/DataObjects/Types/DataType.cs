using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.Types
{
    /// <summary>
    /// Represents the supported data types
    /// </summary>
    internal sealed class DataType
    {
        /// <summary>
        /// Backing field for <see cref="SqlType"/>
        /// </summary>
        private string _sqlType;

        /// <summary>
        /// Gets or sets the sql type
        /// </summary>
        [Appearance(Name = "SQL Type")]
        public string SqlType
        {
            get => _sqlType.ToUpper();
            set => _sqlType = value;
        }

        /// <summary>
        /// Gets or sets the C# type
        /// </summary>
        [Appearance(Name = "C# Type")]
        public string CSharpType { get; set; }
    }
}
