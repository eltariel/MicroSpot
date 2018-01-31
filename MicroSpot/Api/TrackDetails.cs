using System;
using System.Linq;
using SpotifyAPI.Local.Models;
using SpotifyAPI.Web.Models;

namespace MicroSpot.Api
{
    public class TrackDetails
    {
        public TrackDetails(Track track)
        {
            Name = track?.TrackResource.Name ?? "No track";
            Artist = track?.ArtistResource.Name ?? string.Empty;
            Length = TimeSpan.FromSeconds(track?.Length ?? 0);
        }

        public TrackDetails(FullTrack track)
        {
            Name = track?.Name ?? "No track";
            Artist = string.Join(", ", track?.Artists.Select(a => a.Name) ?? new string[0]);
            Length = TimeSpan.FromMilliseconds(track?.DurationMs ?? 0);
        }

        public string Name { get; }
        public string Artist { get; }
        public TimeSpan Length { get; } // TODO: Make this a TimeSpan
    }
}