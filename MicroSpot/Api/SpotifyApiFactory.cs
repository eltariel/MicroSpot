using MicroSpot.Settings;

namespace MicroSpot.Api
{
    public class SpotifyApiFactory
    {
        public static ISpotifyApi GetSpotifyApi(CommsSettings settings)
        {
            return settings.UseWebApi
                ? (ISpotifyApi)new SpotifyWebApi(settings)
                : (ISpotifyApi)new SpotifyLocalApi();
        }
    }
}