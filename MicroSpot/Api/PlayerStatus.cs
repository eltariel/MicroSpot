namespace MicroSpot.Api
{
    public class PlayerStatus
    {
        public PlayerStatus(bool isPlaying, TrackDetails track)
        {
            IsPlaying = isPlaying;
            Track = track;
        }

        public bool IsPlaying { get; }
        public TrackDetails Track { get; }
    }
}