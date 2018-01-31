namespace MicroSpot.Api
{
    public class TrackChangeEventArgs
    {
        public TrackChangeEventArgs(TrackDetails newTrack, TrackDetails oldTrack)
        {
            NewTrack = newTrack;
            OldTrack = oldTrack;
        }

        public TrackDetails OldTrack { get; }

        public TrackDetails NewTrack { get; }
    }
}