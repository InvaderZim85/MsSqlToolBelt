namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents a reference entry of the package config file
    /// </summary>
    internal sealed class ReferenceEntry
    {
        /// <summary>
        /// Gets or sets the name of the package
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the package is a development dependency
        /// </summary>
        public bool IsDevelopmentDependency { get; set; }

        /// <summary>
        /// Gets or sets the target framework
        /// </summary>
        public string TargetFramework { get; set; }
    }
}
