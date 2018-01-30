using System.Windows.Media;
using SpotifyAPI.Web.Models;

namespace MicroSpot
{
    public class Settings
    {
        public Settings()
        {
            Ui = new UiSettings();
            Comms = new CommsSettings();
        }

        public UiSettings Ui { get; set; }
        public CommsSettings Comms { get; set; }
    }

    public class CommsSettings
    {
        public bool UseWebApi { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public Token AuthToken { get; set; }
    }

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