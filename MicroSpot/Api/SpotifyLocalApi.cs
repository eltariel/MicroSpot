using System;
using System.Drawing;
using System.Threading.Tasks;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;

namespace MicroSpot.Api
{
    public class SpotifyLocalApi : ISpotifyApi
    {
        private SpotifyLocalAPI localApi;

        public event EventHandler<TrackChangeEventArgs> OnTrackChange;
        public event EventHandler<PlayStateEventArgs> OnPlayStateChange;
        public event EventHandler<TrackTimeChangeEventArgs> OnTrackTimeChange;

        public void Connect()
        {
            localApi = new SpotifyLocalAPI();
            if (!SpotifyLocalAPI.IsSpotifyRunning() ||
                !SpotifyLocalAPI.IsSpotifyWebHelperRunning() ||
                !localApi.Connect())
            {
                return;
            }

            localApi.OnPlayStateChange += OnLocalPlayStateChange;
            localApi.OnTrackChange += OnLocalTrackChange;
            localApi.OnTrackTimeChange += OnLocalTrackTimeChange;
            localApi.ListenForEvents = true;
        }

        public PlayerStatus GetStatus()
        {
            var status = localApi.GetStatus();
            return new PlayerStatus(status.Playing, new TrackDetails(status.Track));
        }

        private void OnLocalPlayStateChange(object sender, SpotifyAPI.Local.PlayStateEventArgs e)
        {
            OnPlayStateChange?.Invoke(sender, new PlayStateEventArgs(e.Playing));
        }

        private void OnLocalTrackChange(object sender, SpotifyAPI.Local.TrackChangeEventArgs e)
        {
            OnTrackChange?.Invoke(
                sender,
                new TrackChangeEventArgs(
                    new TrackDetails(e.NewTrack),
                    new TrackDetails(e.OldTrack)));
        }

        private void OnLocalTrackTimeChange(object sender, SpotifyAPI.Local.TrackTimeChangeEventArgs e)
        {
            OnTrackTimeChange?.Invoke(sender, new TrackTimeChangeEventArgs(TimeSpan.FromMilliseconds(e.TrackTime)));
        }

        public void Previous()
        {
            localApi.Previous();
        }

        public Task Pause()
        {
            return localApi.Pause();
        }

        public Task Play()
        {
            return localApi.Play();
        }

        public void Skip()
        {
            localApi.Skip();
        }

        public Task<Bitmap> GetAlbumArtAsync(AlbumArtSize size)
        {
            return localApi.GetStatus().Track.GetAlbumArtAsync(size);
        }
    }
}