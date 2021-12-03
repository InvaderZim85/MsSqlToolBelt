using ZimLabs.WpfBase;

namespace MsSqlToolBelt.DataObjects.DefinitionExport
{
    /// <summary>
    /// Represents an entry which is used for the definition export
    /// </summary>
    internal sealed class DefinitionEntry : ObservableObject
    {
        /// <summary>
        /// Gets or sets the name of the according schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the entry
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Backing field for <see cref="Export"/>
        /// </summary>
        private bool _export;

        /// <summary>
        /// Gets or sets the value which indicates the entry should be exported
        /// </summary>
        public bool Export
        {
            get => _export;
            set => SetField(ref _export, value);
        }
    }
}
