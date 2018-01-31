namespace MicroSpot.Api
{
    public class PlayStateEventArgs
    {
        public PlayStateEventArgs(bool isPlaying)
        {
            IsPlaying = isPlaying;
        }

        public bool IsPlaying { get; }
    }
}