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
        private readonly AuthHelper auth;
        private readonly Timer eventPollTimer;

        private SpotifyWebAPI webApi;

        private PlayerStatus status;
        private PlaybackContext lastPlayback;
        private (string imgUrl, Bitmap bitmap) currentImg;

        public SpotifyWebApi(Configuration config)
        {
            auth = new AuthHelper(config);
            eventPollTimer = new Timer(OnEventPollTick);
        }

        public event EventHandler<TrackChangeEventArgs> OnTrackChange;
        public event EventHandler<PlayStateEventArgs> OnPlayStateChange;
        public event EventHandler<TrackTimeChangeEventArgs> OnTrackTimeChange;

        public bool IsConnected { get; private set; }

        public void Connect()
        {
            auth.Authorise();
            webApi = new SpotifyWebAPI();
            eventPollTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(0.5));
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
            var imgUrl = lastPlayback?.Item?.Album.Images[0].Url;
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

        private void OnEventPollTick(object state)
        {
            if (!auth.SetAuth(webApi)) return;
            IsConnected = true;

            var ls = status;
            status = GetStatus();

            if (status.IsPlaying != ls?.IsPlaying)
            {
                OnPlayStateChange?.Invoke(this, new PlayStateEventArgs(status.IsPlaying));
            }

            if (status.Track != ls?.Track)
            {
                OnTrackChange?.Invoke(this, new TrackChangeEventArgs(status.Track, ls?.Track));
            }

            if (status.PlayPosition != ls?.PlayPosition)
            {
                OnTrackTimeChange?.Invoke(this, new TrackTimeChangeEventArgs(status.PlayPosition));
            }
        }
    }
}