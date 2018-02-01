using MicroSpot.Settings;

namespace MicroSpot.Api
{
    public class SpotifyApiFactory
    {
        public static ISpotifyApi GetSpotifyApi(Configuration config)
        {
            return config.Comms.UseWebApi
                ? (ISpotifyApi)new SpotifyWebApi(config)
                : (ISpotifyApi)new SpotifyLocalApi();
        }
    }
}