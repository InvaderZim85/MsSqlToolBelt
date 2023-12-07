using System.IO.Compression;
using Serilog;
using Serilog.Events;
using System.Xml.Linq;
using ZimLabs.DotnetReleaseHelper;
using ZimLabs.DotnetReleaseHelper.Common.Enum;

namespace MsSqlToolBeltPublisher;

/// <summary>
/// Provides the main logic
/// </summary>
internal static class Program
{
    /// <summary>
    /// Provides the main entry point of the program
    /// </summary>
    private static void Main()
    {
        var releaseHelper = new ReleaseHelper(LogEventLevel.Debug);

        Log.Information("==== Publisher - Start ====");

        var rootDir = GetRootDir();

        if (string.IsNullOrEmpty(rootDir))
        {
            Log.Error("Can't determine root directory");
            return;
        }

        // Create the settings
        var settings = new ReleaseSettings
        {
            SolutionFile = LoadPath(rootDir, PathType.SolutionFile),
            ProjectFile = LoadPath(rootDir, PathType.ProjectFile),
            PublishProfileFile = LoadPath(rootDir, PathType.PublishProfileFile),
            BinDir = LoadPath(rootDir, PathType.BinDir),
            CleanBin = true,
            VersionType = VersionType.VersionWithDayOfYear,
            CreateZipArchive = true,
            ZipArchiveName = "MsSqlToolBelt",
            AttachVersionToZipArchiveName = true,
            CustomActions =
            [
                new CustomAction
                {
                    Name = "ExtractNugetPackages",
                    Action = ExtractPackages,
                    ExecutionType = ActionExecutionType.BeforePublish,
                    StopOnException = true
                }
            ],
            ZipCompressionLevel = CompressionLevel.Optimal
        };

        // Create the release
        releaseHelper.CreateRelease(settings);

        Log.Information("==== Publisher - Done ====");
    }

    /// <summary>
    /// Loads the desired path
    /// </summary>
    /// <param name="rootDir">The path of the "root" directory</param>
    /// <param name="type">The desired path which should be loaded</param>
    /// <returns>The path</returns>
    private static string LoadPath(string rootDir, PathType type)
    {
        var dirInfo = new DirectoryInfo(rootDir);

        var files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        var subDirs = dirInfo.GetDirectories("*", SearchOption.AllDirectories);

        return type switch
        {
            PathType.SolutionFile =>
                files.FirstOrDefault(f => f.Extension.Equals(".sln", StringComparison.OrdinalIgnoreCase))
                    ?.FullName ?? string.Empty,
            PathType.ProjectFile =>
                files.FirstOrDefault(f => f.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                    ?.FullName ?? string.Empty,
            PathType.PublishProfileFile =>
                files.FirstOrDefault(f => f.Extension.Equals(".pubxml", StringComparison.OrdinalIgnoreCase))
                    ?.FullName ?? string.Empty,
            PathType.BinDir => subDirs.FirstOrDefault(f => f.Name.Contains("bin"))?.FullName ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Extracts the packages and stores them into the desired directory
    /// </summary>
    /// <param name="settings">The settings</param>
    private static void ExtractPackages(ReleaseSettings settings)
    {
        var xmlDoc = XDocument.Load(settings.ProjectFile);

        var packages = (from element in xmlDoc.Descendants()
            where element.Name.LocalName.Equals("PackageReference")
            let package = element?.Attribute("Include")?.Value ?? string.Empty
            let version = element?.Attribute("Version")?.Value ?? string.Empty
            where !string.IsNullOrEmpty(package) &&
                  !string.IsNullOrEmpty(version)
            select $"{package};{version}").ToList();

        // Create the path
        var path = Path.GetDirectoryName(settings.ProjectFile) ?? string.Empty;
        if (string.IsNullOrEmpty(path))
        {
            Log.Error("Can't determine package file path.");
            return;
        }

        var destination = Path.Combine(path, "Packages.csv");

        // Export the data
        Log.Information("{count} packages extracted.", packages.Count);
        File.WriteAllLines(destination, packages);
    }

    /// <summary>
    /// Gets the root directory
    /// </summary>
    /// <returns>The path of the root directory</returns>
    private static string GetRootDir()
    {
        return GetDirPath(AppContext.BaseDirectory, "MsSqlToolBelt.sln");
    }

    /// <summary>
    /// Gets the path of the directory which contains the desired file
    /// </summary>
    /// <param name="rootDir">The path of the root directory</param>
    /// <param name="filename">The name of the file</param>
    /// <returns>The found path</returns>
    private static string GetDirPath(string rootDir, string filename)
    {
        Log.Debug("Check path '{path}'", rootDir);

        // Step 1: Check if we can find the find underneath us
        var dirInfo = new DirectoryInfo(rootDir);
        var files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        var tmpFile = files.FirstOrDefault(f => f.Name.Equals(filename, StringComparison.OrdinalIgnoreCase));
        if (tmpFile != null)
        {
            // We've found the file
            return Path.GetDirectoryName(tmpFile.FullName) ?? string.Empty;
        }

        // We've to go a level up
        return dirInfo.Parent != null ? GetDirPath(dirInfo.Parent.FullName, filename) : string.Empty;
    }
}