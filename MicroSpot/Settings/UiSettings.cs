using System.Windows.Media;

namespace MicroSpot.Settings
{
    public class UiSettings
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        public Brush ProgressBrush { get; set; }
        public Brush TrackTitleBrush { get; set; }
        public Brush TrackArtistBrush { get; set; }
        public Brush TimeBrush { get; set; }
        public Brush BackgroundBrush { get; set; }
        public bool DarkIcons { get; set; }
    }
}