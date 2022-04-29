using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MsSqlToolBelt.DataObjects;

namespace MsSqlToolBelt.Business
{
    /// <summary>
    /// Provides functions for the interaction with the package information
    /// </summary>
    internal static class PackageInfo
    {
        /// <summary>
        /// Gets the package information of the project
        /// </summary>
        /// <returns>The list with the reference data</returns>
        public static List<ReferenceEntry> GetPackageInformation()
        {
            var packageFile = Path.Combine(Helper.GetBaseFolder(), "packages.config");
            if (!File.Exists(packageFile))
                return new List<ReferenceEntry>();

            var doc = XDocument.Load(packageFile);

            // Get the values
            var result = (from entry in doc.Descendants("package")
                let id = entry.Attribute("id")?.Value
                let version = entry.Attribute("version")?.Value
                let targetFramework = entry.Attribute("targetFramework")?.Value
                let developmentDependency = entry.Attribute("developmentDependency")?.Value
                select new ReferenceEntry
                {
                    Name = id,
                    Version = version,
                    TargetFramework = targetFramework,
                    IsDevelopmentDependency = developmentDependency.ToBool()
                }).ToList();

            return result;
        }
    }
}
