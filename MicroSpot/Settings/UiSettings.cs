using System.Windows.Media;

namespace MicroSpot.Settings
{
    public class UiSettings
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        public Color ProgressColor { get; set; }
        public Color TrackTitleColor { get; set; }
        public Color TrackArtistColor { get; set; }
        public Color TimeColor { get; set; }
        public Color BackgroundColor { get; set; }
        public bool DarkIcons { get; set; }
    }
}