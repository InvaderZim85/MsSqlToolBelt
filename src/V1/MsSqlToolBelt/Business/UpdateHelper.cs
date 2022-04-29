using System;
using System.Reflection;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Github;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;

namespace MsSqlToolBelt.Business
{
    /// <summary>
    /// Provides REST helper functions
    /// </summary>
    internal static class UpdateHelper
    {
        /// <summary>
        /// Contains the url to get the latest release of the MsSqlToolBelt
        /// </summary>
        private const string GitHubApiUrl = "https://api.github.com/repos/InvaderZim85/MsSqlToolBelt/releases/latest";

        /// <summary>
        /// Loads the release info of the latest release
        /// </summary>
        /// <param name="callback"></param>
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

                if (NewVersionAvailable(response))
                    callback(response);
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
        /// <returns>true when an update is available, otherwise false</returns>
        private static bool NewVersionAvailable(ReleaseInfo releaseInfo)
        {
            if (releaseInfo == null)
                return false;

            if (!Version.TryParse(releaseInfo.TagName.Replace("v", ""), out var releaseVersion))
            {
                Log.Warning("Can't determine version of the latest release. Value: {version}", releaseInfo.TagName);
                return false;
            }

            // Get the version number of the current version
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            releaseInfo.CurrentVersion = currentVersion;
            releaseInfo.NewVersion = releaseVersion;

            return releaseVersion > currentVersion;
        }
    }
}
