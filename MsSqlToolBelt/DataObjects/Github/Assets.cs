using Newtonsoft.Json;

namespace MsSqlToolBelt.DataObjects.Github
{
    /// <summary>
    /// Provides the assets of the last release
    /// </summary>
    public sealed class Assets
    {
        /// <summary>
        /// Gets or sets the name of the zip file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size of the latest release (in bytes)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the url of the release
        /// </summary>
        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
}
