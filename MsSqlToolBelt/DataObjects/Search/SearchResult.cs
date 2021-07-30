using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Gets the name of the procedure / table (only needed for the view)
        /// </summary>
        public string NameView
        {
            get
            {
                Info = IsTable && Columns.Any(a => a.IsReplicated) ? "Marked for replication" : "";

                return Name;
            }
        }

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

        /// <summary>
        /// Gets or sets the info of the entry
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Gets or sets the creation date / time
        /// </summary>
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modification date / time
        /// </summary>
        public DateTime ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets the value which indicates if the result represents a table
        /// </summary>
        public bool IsTable => Type.Equals("Table");

        /// <summary>
        /// Gets or sets the list of table columns
        /// </summary>
        public List<TableColumn> Columns { get; set; } = new();

        /// <summary>
        /// Gets or sets the list with the indices
        /// </summary>
        public List<TableIndex> Indices { get; set; } = new();
    }
}
