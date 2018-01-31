using SpotifyAPI.Web.Models;

namespace MicroSpot.Settings
{
    public class CommsSettings
    {
        public bool UseWebApi { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public Token AuthToken { get; set; }
    }
}