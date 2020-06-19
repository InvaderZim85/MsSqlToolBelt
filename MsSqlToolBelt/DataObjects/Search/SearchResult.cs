using ZimLabs.WpfBase;

namespace MsSqlToolBelt.DataObjects.Search
{
    /// <summary>
    /// Represents a single search result
    /// </summary>
    internal sealed class SearchResult : ObservableObject
    {
        /// <summary>
        /// Gets or sets the id of the object
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the procedure / table
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the definition of the search result
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// Gets or sets the type of the search result
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the step id (only for job steps)
        /// </summary>
        public int StepId { get; set; }

        /// <summary>
        /// Gets or sets the step name (only for job steps)
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// Gets or sets the id of the success action (only for job steps)
        /// </summary>
        public int SuccessAction { get; set; }

        /// <summary>
        /// Gets or sets the id of the next step in case of success
        /// </summary>
        public int SuccessStepId { get; set; }

        /// <summary>
        /// Gets or sets the id of the fail action (only for job steps)
        /// </summary>
        public int FailAction { get; set; }

        /// <summary>
        /// Gets or sets the id of the next step in case of failure
        /// </summary>
        public int FailStepId { get; set; }

        /// <summary>
        /// Backing field for <see cref="Export"/>
        /// </summary>
        private bool _export;

        /// <summary>
        /// Gets or sets the value which indicates if the entry should be exported
        /// </summary>
        public bool Export
        {
            get => _export;
            set => SetField(ref _export, value);
        }
    }
}
