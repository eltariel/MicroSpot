using System;
using System.Windows;
using System.Windows.Input;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;

namespace MicroSpot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SpotifyLocalAPI spotify;
        private Track currentTrack;

        public MainWindow()
        {
            InitializeComponent();

            spotify = new SpotifyLocalAPI();
            if (!SpotifyLocalAPI.IsSpotifyRunning() ||
                !SpotifyLocalAPI.IsSpotifyWebHelperRunning() ||
                !spotify.Connect())
            {
                return;
            }

            var status = spotify.GetStatus();
            UpdateDisplay(status.Track);

            spotify.OnTrackChange += OnTrackChange;
            spotify.OnPlayStateChange += OnPlayStateChange;
            spotify.OnTrackTimeChange += OnTrackTimeChange;
            spotify.ListenForEvents = true;
        }

        private void OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnPlayStateChange(sender, e)));
                return;
            }

            PlayButton.Content = spotify.GetStatus().Playing ? "||" : "|>";
        }

        private void OnTrackChange(object sender, TrackChangeEventArgs e)
        {
            UpdateDisplay(e.NewTrack);
        }

        private void OnTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnTrackTimeChange(sender, e)));
                return;
            }

            TrackTime.Text = $@"{TimeSpan.FromSeconds(e.TrackTime):m\:ss}/{TimeSpan.FromSeconds(currentTrack.Length):m\:ss}";
            if (e.TrackTime < (currentTrack?.Length ?? 0))
            {
                TrackProgress.Value = (int) e.TrackTime;
            }
        }

        private async void UpdateDisplay(Track track)
        {
            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.BeginInvoke(new Action(() => UpdateDisplay(track)));
                return;
            }

            currentTrack = track;
            TrackTitle.Text = track?.TrackResource.Name ?? "No track";
            TrackArtist.Text = track?.ArtistResource.Name ?? string.Empty;
            TrackImage.Source = track?.AlbumResource != null ? (await track.GetAlbumArtAsync(AlbumArtSize.Size160)).ToBitmapSource() : null;
            TrackProgress.Maximum = track?.Length ?? 0;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            spotify.Previous();
        }

        private async void OnPlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (spotify.GetStatus().Playing)
            {
                await spotify.Pause();
            }
            else
            {
                await spotify.Play();
            }
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            spotify.Skip();
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
