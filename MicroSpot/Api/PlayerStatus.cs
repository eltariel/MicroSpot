using System;

namespace MicroSpot.Api
{
    public class PlayerStatus
    {
        public PlayerStatus(bool isPlaying, TrackDetails track, TimeSpan playPosition)
        {
            IsPlaying = isPlaying;
            Track = track;
            PlayPosition = playPosition;
        }

        public bool IsPlaying { get; }
        public TrackDetails Track { get; }
        public TimeSpan PlayPosition { get; }
    }
}