using System.Collections.Generic;
using MsSqlToolBelt.DataObjects.Search;
using ZimLabs.TableCreator;

namespace MsSqlToolBelt.DataObjects.ClassGenerator
{
    /// <summary>
    /// Represents a table
    /// </summary>
    internal sealed class Table
    {
        /// <summary>
        /// Gets or sets the name of the table
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the schema
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the list with the columns
        /// </summary>
        public List<TableColumn> Columns { get; set; }

        /// <summary>
        /// Gets or sets the list with the indices
        /// </summary>
        [Appearance(Ignore = true)]
        public List<TableIndex> Indices { get; set; } = new();

        /// <summary>
        /// Converts the given <see cref="SearchResult"/> into a <see cref="Table"/>
        /// </summary>
        /// <param name="result">The search result entry</param>
        public static explicit operator Table(SearchResult result)
        {
            return new()
            {
                Name = result.Name,
                Schema = result.Schema,
                Columns = result.Columns,
                Indices = result.Indices
            };
        }
    }
}
