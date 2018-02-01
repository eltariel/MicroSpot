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
using SpotifyAPI.Web.Models;

namespace MicroSpot.Api
{
    public class SpotifyWebApi : ISpotifyApi
    {
        private readonly Configuration config;
        private readonly AutorizationCodeAuth auth;
        private SpotifyWebAPI webApi;
        private Timer tokenRefreshTimer;
        private Timer eventPollTimer;
        private PlayerStatus lastStatus;
        private PlaybackContext lastPlayback;
        private (string imgUrl, Bitmap bitmap) currentImg;

        public SpotifyWebApi(Configuration config)
        {
            this.config = config;

            auth = new AutorizationCodeAuth
            {
                ClientId = config.Comms.ClientId,
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
            tokenRefreshTimer = new Timer(OnTokenRefreshTick);
            eventPollTimer = new Timer(OnEventPollTick);

            if (config.Comms.AuthToken == null)
            {
                auth.StartHttpServer(8888);
                auth.DoAuth();
            }
            else
            {
                RefreshToken();
            }

            eventPollTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void OnEventPollTick(object state)
        {
            var s = GetStatus();

            if (s.IsPlaying != lastStatus?.IsPlaying)
            {
                OnPlayStateChange?.Invoke(this, new PlayStateEventArgs(s.IsPlaying));
            }

            if (s.Track != lastStatus?.Track)
            {
                OnTrackChange?.Invoke(this, new TrackChangeEventArgs(s.Track, lastStatus?.Track));
            }

            if (s.PlayPosition != lastStatus?.PlayPosition)
            {
                OnTrackTimeChange?.Invoke(this, new TrackTimeChangeEventArgs(s.PlayPosition));
            }

            lastStatus = s;
        }

        public PlayerStatus GetStatus()
        {
            lastPlayback = webApi.GetPlayback();

            return lastPlayback.HasError()
                ? new PlayerStatus(false, new TrackDetails(default(FullTrack)), TimeSpan.Zero)
                : new PlayerStatus(lastPlayback.IsPlaying, new TrackDetails(lastPlayback.Item), TimeSpan.FromMilliseconds(lastPlayback.ProgressMs));
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
            var imgUrl = lastPlayback?.Item.Album.Images[0].Url;
            Bitmap bitmap;

            if (currentImg.imgUrl == imgUrl && currentImg.bitmap != null)
            {
                return currentImg.bitmap;
            }

            if (imgUrl == null)
            {
                bitmap = new Bitmap(1,1);
            }
            else
            {
                using (var wc = new WebClient())
                using (var s = await wc.OpenReadTaskAsync(imgUrl))
                {
                    bitmap = new Bitmap(s);
                }
            }

            currentImg = (imgUrl, bitmap);

            return bitmap;
        }

        private void OnAuthResponseReceived(AutorizationCodeAuthResponse response)
        {
            var token = auth.ExchangeAuthCode(response.Code, config.Comms.ClientSecret);

            if (string.IsNullOrWhiteSpace(token.Error))
            {
                config.Comms.AuthToken = token;
                RefreshToken();
            }
            else
            {
                MessageBox.Show($"Error authenticating: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
            }

            auth.StopHttpServer();
        }

        private void OnTokenRefreshTick(object state)
        {
            RefreshToken();
        }

        private void RefreshToken()
        {
            lock (auth)
            {
                var token = auth.RefreshToken(config.Comms.AuthToken.RefreshToken, config.Comms.ClientSecret);

                if (string.IsNullOrWhiteSpace(token.Error))
                {
                    if (string.IsNullOrWhiteSpace(token.RefreshToken))
                    {
                        // Refresh token may not always be replaced.
                        token.RefreshToken = config.Comms.AuthToken.RefreshToken;
                    }

                    config.Comms.AuthToken = token;
                    config.WriteComms();

                    webApi.TokenType = config.Comms.AuthToken.TokenType;
                    webApi.AccessToken = config.Comms.AuthToken.AccessToken;
                }
                else
                {
                    MessageBox.Show($"Error refreshing token: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
                }
            }
            
            // ReSharper disable once PossibleLossOfFraction
            tokenRefreshTimer.Change(
                TimeSpan.FromSeconds(config.Comms.AuthToken.ExpiresIn / 2),
                Timeout.InfiniteTimeSpan);
        }
    }
}