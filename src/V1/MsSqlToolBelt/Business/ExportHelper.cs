using System;
using System.Collections.Generic;
using System.IO;
using MsSqlToolBelt.DataObjects.Search;

namespace MsSqlToolBelt.Business
{
    /// <summary>
    /// Provides several export functions
    /// </summary>
    internal static class ExportHelper
    {
        /// <summary>
        /// Exports the given result list into the specified destination
        /// </summary>
        /// <param name="destination">The destination</param>
        /// <param name="resultList">The list with the results</param>
        public static void ExportSearchResult(string destination, List<SearchResult> resultList)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentNullException(nameof(destination));

            if (resultList is null)
                throw new ArgumentNullException(nameof(resultList));

            var files = new List<string>();

            foreach (var entry in resultList)
            {
                files.Add(entry.Name);

                var path = Path.Combine(destination, $"{GetValidFileName(entry.Name)}.sql");

                File.WriteAllText(path, entry.Definition);
            }

            File.WriteAllLines(Path.Combine(destination, "Files.txt"), files);
        }

        /// <summary>
        /// Removes all invalid chars from the file name
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The valid file name</returns>
        private static string GetValidFileName(string value)
        {
            value = value.Replace(" ", "_");
            return value;
        }
    }
}
