using System;
using System.Threading.Tasks;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;

namespace MicroSpot
{
    public class SpotifyApiProxy
    {
        private readonly CommsSettings commsSettings;
        private SpotifyLocalAPI localApi;

        public SpotifyApiProxy(CommsSettings commsSettings)
        {
            this.commsSettings = commsSettings;
        }

        public event EventHandler<TrackChangeEventArgs> OnTrackChange;
        public event EventHandler<PlayStateEventArgs> OnPlayStateChange;
        public event EventHandler<TrackTimeChangeEventArgs> OnTrackTimeChange;

        public void Connect()
        {
            //TODO: if (!commsSettings.UseWebApi)
            if (true)
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
            else
            {
                // Do web API connect instead
            }
        }

        public StatusResponse GetStatus()
        {
            return localApi.GetStatus();
        }

        private void OnLocalPlayStateChange(object sender, PlayStateEventArgs e)
        {
            OnPlayStateChange?.Invoke(sender, e);
        }

        private void OnLocalTrackChange(object sender, TrackChangeEventArgs e)
        {
            OnTrackChange?.Invoke(sender, e);
        }

        private void OnLocalTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            OnTrackTimeChange?.Invoke(sender, e);
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
    }
}