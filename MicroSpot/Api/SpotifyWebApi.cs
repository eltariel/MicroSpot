using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MicroSpot.Settings;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;

namespace MicroSpot.Api
{
    public class SpotifyWebApi : ISpotifyApi
    {
        private readonly CommsSettings commsSettings;
        private readonly AutorizationCodeAuth auth;
        private SpotifyWebAPI webApi;
        private Timer tokenRefreshTimer;

        public SpotifyWebApi(CommsSettings commsSettings)
        {
            this.commsSettings = commsSettings;

            auth = new AutorizationCodeAuth
            {
                ClientId = commsSettings.ClientId,
                RedirectUri = "http://localhost:8888",
                Scope = (Scope)0x1FFFF,
            };
            auth.OnResponseReceivedEvent += OnAuthResponseReceived;
        }

        public event EventHandler<TrackChangeEventArgs> OnTrackChange;
        public event EventHandler<PlayStateEventArgs> OnPlayStateChange;
        public event EventHandler<TrackTimeChangeEventArgs> OnTrackTimeChange;

        public void Connect()
        {
            webApi = new SpotifyWebAPI();
            tokenRefreshTimer = new Timer(TokenRefreshTimerCallback);

            if (commsSettings.AuthToken == null)
            {
                auth.StartHttpServer(8888);
                auth.DoAuth();
            }
            else
            {
                RefreshToken();
            }

            // Start event poll timer
        }

        public PlayerStatus GetStatus()
        {
            var playbackContext = webApi.GetPlayback();
            return new PlayerStatus(playbackContext.IsPlaying, new TrackDetails(playbackContext.Item));
        }

        public void Previous()
        {
            webApi.SkipPlaybackToPrevious();
        }

        public Task Pause()
        {
            webApi.PausePlayback();
            return Task.CompletedTask;
        }

        public Task Play()
        {
            webApi.ResumePlayback();
            return Task.CompletedTask;
        }

        public void Skip()
        {
            webApi.SkipPlaybackToNext();
        }

        public async Task<Bitmap> GetAlbumArtAsync(AlbumArtSize size)
        {
            // TODO: Select best img size
            // TODO: Cache this
            var imgUrl = webApi.GetPlayback().Item.Album.Images[0].Url;
            Bitmap b;
            using (var wc = new WebClient())
            using (var s = await wc.OpenReadTaskAsync(imgUrl))
            {
                b = new Bitmap(s);
            }

            return b;
        }

        private void OnAuthResponseReceived(AutorizationCodeAuthResponse response)
        {
            var token = auth.ExchangeAuthCode(response.Code, commsSettings.ClientSecret);

            if (string.IsNullOrWhiteSpace(token.Error))
            {
                commsSettings.AuthToken = token;

                webApi.TokenType = commsSettings.AuthToken.TokenType;
                webApi.AccessToken = commsSettings.AuthToken.AccessToken;
            }
            else
            {
                MessageBox.Show($"Error authenticating: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
            }

            auth.StopHttpServer();
        }

        private void TokenRefreshTimerCallback(object state)
        {
            RefreshToken();
        }

        private void RefreshToken()
        {
            commsSettings.AuthToken = auth.RefreshToken(commsSettings.AuthToken.RefreshToken, commsSettings.ClientSecret);
            webApi.TokenType = commsSettings.AuthToken.TokenType;
            webApi.AccessToken = commsSettings.AuthToken.AccessToken;
            // ReSharper disable once PossibleLossOfFraction
            tokenRefreshTimer.Change(TimeSpan.FromSeconds(commsSettings.AuthToken.ExpiresIn / 2), Timeout.InfiniteTimeSpan);
        }
    }
}