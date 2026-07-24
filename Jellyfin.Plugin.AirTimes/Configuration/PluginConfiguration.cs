using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AirTimes.Configuration;

/// <inheritdoc/>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the UTC timestamp of the last successful run of the premiere date fix task.
    /// </summary>
    public DateTime LastPremiereDateFixRunUtc { get; set; } = DateTime.MinValue;
}