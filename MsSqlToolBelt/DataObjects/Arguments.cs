using CommandLine;

namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Provides the shipped arguments
    /// </summary>
    internal sealed class Arguments
    {
        /// <summary>
        /// Gets or sets the name / path of the database server
        /// </summary>
        [Option('s', "server", Required = false, HelpText = "The name / path of the server")]
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the name of the desired database
        /// </summary>
        [Option('d', "database", Required = false, HelpText = "The name of the desired database")]
        public string Database { get; set; }

        /// <summary>
        /// Returns the name / path of the server and the name of the database
        /// </summary>
        /// <returns>The name / path of the server and the name of the database</returns>
        public override string ToString()
        {
            return $"Server: '{Server}'; Database: '{Database}'";
        }
    }
}
