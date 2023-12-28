using Newtonsoft.Json;

namespace MsSqlToolBelt.DataObjects.Updater;

/// <summary>
/// Provides the information of the latest release
/// </summary>
public class ReleaseInfo
{
    /// <summary>
    /// Gets or sets the html url of the release
    /// </summary>
    [JsonProperty("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the tag
    /// </summary>
    [JsonProperty("tag_name")]
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the release
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publish date of the release
    /// </summary>
    [JsonProperty("published_at")]
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Gets the published at date as formatted string
    /// </summary>
    [JsonIgnore]
    public string PublishedAtView => PublishedAt.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Gets or sets the body (message) of the release
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the with the assets
    /// </summary>
    public List<Asset> Assets { get; set; } = [];

    /// <summary>
    /// Gets or sets the number current version
    /// </summary>
    public Version CurrentVersion { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of the new version
    /// </summary>
    public Version NewVersion { get; set; } = new();
}