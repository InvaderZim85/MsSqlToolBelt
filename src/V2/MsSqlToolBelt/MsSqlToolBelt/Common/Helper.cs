using System.IO;
using System.Reflection;
using Serilog;
using Log = Serilog.Log;

namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides several helper functions
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Gets the path of the base directory
    /// </summary>
    /// <returns>The path of the base directory</returns>
    public static string GetBaseDirPath()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(assemblyPath) ?? "";
    }

    /// <summary>
    /// Init the logger
    /// </summary>
    public static void InitLogger()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.SQLite("logs/Log.db").CreateLogger();
    }
}