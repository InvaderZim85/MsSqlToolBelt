namespace MsSqlToolBeltPublisher;

/// <summary>
/// Provides the different file types
/// </summary>
internal enum PathType
{
    /// <summary>
    /// Determines the path of the solution file
    /// </summary>
    SolutionFile,

    /// <summary>
    /// Determines the path of the project file
    /// </summary>
    ProjectFile,

    /// <summary>
    /// Determines the path of the publish profile
    /// </summary>
    PublishProfileFile,

    /// <summary>
    /// Determines the path of the bin directory
    /// </summary>
    BinDir
}