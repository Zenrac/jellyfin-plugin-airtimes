using System;

namespace Jellyfin.Plugin.AirTimes.Helpers;

/// <summary>
/// Helpers for computing episode premiere dates.
/// </summary>
public static class PremiereDateHelper
{
    /// <summary>
    /// Clamp an air date so it cannot be later than the date added (or today).
    /// </summary>
    public static DateTime ClampFutureDate(DateTime airDate, DateTime? dateAdded)
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
}