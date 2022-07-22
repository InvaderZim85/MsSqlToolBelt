using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Updater;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;

namespace MsSqlToolBelt.Business;

/// <summary>
/// Provides the functions for the update process
/// </summary>
internal static class UpdateHelper
{
    /// <summary>
    /// Contains the url of the latest release of the MsSqlToolBelt
    /// </summary>
    private const string GitHubApiUrl = "https://api.github.com/repos/InvaderZim85/MsSqlToolBelt/releases/latest";

    /// <summary>
    /// Loads the infos of the latest release
    /// </summary>
    /// <param name="callback">The callback action to inform the user about the available update</param>
    /// <returns>The awaitable task</returns>
    public static async Task LoadReleaseInfoAsync(Action<ReleaseInfo> callback)
    {
        try
        {
            var client = new RestClient(GitHubApiUrl);
            client.AddDefaultHeader("accept", "application/vnd.github.v3+json");
            client.UseNewtonsoftJson();

            var request = new RestRequest();

            var response = await client.GetAsync<ReleaseInfo>(request);

            // This method also checks if the response is null
            if (IsNewVersionAvailable(response))
                callback(response!);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "An error has occurred while checking the latest release.");
        }
    }

    /// <summary>
    /// Checks if an update is available
    /// </summary>
    /// <param name="releaseInfo">The infos of the latest release</param>
    /// <returns><see langword="true"/> when there is a new version, otherwise <see langword="false"/></returns>
    private static bool IsNewVersionAvailable(ReleaseInfo? releaseInfo)
    {
        if (releaseInfo == null)
            return false;

        if (!Version.TryParse(releaseInfo.TagName.Replace("v", ""), out var releaseVersion))
        {
            Log.Warning("Can't determine version of the latest release. Tag value: {value}", releaseInfo.TagName);
            return false;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (currentVersion == null)
            return false;

        releaseInfo.CurrentVersion = currentVersion;
        releaseInfo.NewVersion = releaseVersion;

        return releaseVersion > currentVersion;
    }
}