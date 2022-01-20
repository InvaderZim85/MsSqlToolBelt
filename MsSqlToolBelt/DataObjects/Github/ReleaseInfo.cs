﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MsSqlToolBelt.DataObjects.Github
{
    /// <summary>
    /// Provides the information of the latest release
    /// </summary>
    public sealed class ReleaseInfo
    {
        /// <summary>
        /// Gets or sets the html url of the release
        /// </summary>
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag
        /// </summary>
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the name of the last release
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the publish date
        /// </summary>
        [JsonProperty("published_at")]
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Gets or sets the body of the release
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the assets of the release
        /// </summary>
        public List<Assets> Assets { get; set; }

        /// <summary>
        /// Gets or sets the number current version
        /// </summary>
        public Version CurrentVersion { get; set; }

        /// <summary>
        /// Gets or sets the number of the new version
        /// </summary>
        public Version NewVersion { get; set; }
    }
}