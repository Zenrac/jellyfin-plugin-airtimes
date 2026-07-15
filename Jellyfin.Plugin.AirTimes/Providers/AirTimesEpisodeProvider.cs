using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using J2N;
using Jellyfin.Plugin.AirTimes.Services;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AirTimes.Providers;

/// <summary>
/// Air Times episodes provider.
/// </summary>
public class AirTimesEpisodeProvider(
  IHttpClientFactory httpClientFactory,
  ILoggerFactory loggerFactory,
  ILibraryManager libraryManager)
  : IRemoteMetadataProvider<Episode, EpisodeInfo>
{
  private readonly ILogger<AirTimesEpisodeProvider> logger = loggerFactory.CreateLogger<AirTimesEpisodeProvider>();
  private readonly Tvdb tvdb = new(httpClientFactory, loggerFactory);

  /// <inheritdoc/>
  public string Name => "TheTVDB (Air Times)";

  /// <inheritdoc/>
  public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
  {
    var metadata = new MetadataResult<Episode>();

    int seriesId = info.SeriesProviderIds.GetPropertyAsInt32("Tvdb");
    if (seriesId is 0)
    {
      logger.LogWarning("Could not retrieve TVDB ID for \"{Name}\"", info.Name);
      return metadata;
    }

    object episodeResolvable = info.ProviderIds.GetPropertyAsInt32("Tvdb") is 0
      ? info.Name
      : info.ProviderIds.GetPropertyAsInt32("Tvdb");
    var episodeAirDate = await tvdb
      .GetEpisodeAirDate(seriesId, episodeResolvable, cancellationToken)
      .ConfigureAwait(false);
    if (episodeAirDate is null)
    {
      logger.LogWarning("Could not retrieve episode air date for \"{Name}\"", episodeResolvable);
      return metadata;
    }

    metadata.Item = new Episode
    {
      PremiereDate = ClampFutureDate(episodeAirDate.Value, GetDateAdded(info))
    };
    metadata.HasMetadata = true;

    return metadata;
  }

  private DateTime? GetDateAdded(EpisodeInfo info)
  {
    if (string.IsNullOrWhiteSpace(info.Path))
    {
      return null;
    }

    return libraryManager.FindByPath(info.Path, false)?.DateCreated
        ?? GetFileDateAdded(info.Path);
  }

  private static DateTime? GetFileDateAdded(string path)
  {
    return File.Exists(path) ? File.GetCreationTimeUtc(path) : null;
  }

  private static DateTime ClampFutureDate(DateTime airDate, DateTime? dateAdded)
  {
    var maxDate = DateTime.UtcNow.Date;
    if (dateAdded.HasValue && dateAdded.Value.Date < maxDate)
    {
      maxDate = dateAdded.Value.Date;
    }

    return airDate.Date > maxDate
      ? maxDate
      : airDate;
  }

  /// <summary>
  /// Not implemented.
  /// </summary>
  public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo info, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Not implemented.
  /// </summary>
  public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
