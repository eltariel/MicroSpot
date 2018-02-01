using System;
using System.Drawing;
using System.Threading.Tasks;
using SpotifyAPI.Local.Enums;

namespace MicroSpot.Api
{
    public interface ISpotifyApi
    {
        event EventHandler<PlayStateEventArgs> OnPlayStateChange;
        event EventHandler<TrackChangeEventArgs> OnTrackChange;
        event EventHandler<TrackTimeChangeEventArgs> OnTrackTimeChange;

        bool IsConnected { get; }

        void Connect();
        Task<Bitmap> GetAlbumArtAsync(AlbumArtSize size);
        PlayerStatus GetStatus();
        Task Pause();
        Task Play();
        void Previous();
        void Skip();
    }
}