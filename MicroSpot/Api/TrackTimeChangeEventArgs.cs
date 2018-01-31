using System;

namespace MicroSpot.Api
{
    public class TrackTimeChangeEventArgs
    {
        public TrackTimeChangeEventArgs(TimeSpan time)
        {
            Time = time;
        }

        public TimeSpan Time { get; }
    }
}