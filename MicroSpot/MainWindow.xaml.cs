using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
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

        public MainWindow()
        {
            InitializeComponent();

            spotify = new SpotifyLocalAPI();
            if (!SpotifyLocalAPI.IsSpotifyRunning())
                return; //Make sure the spotify client is running
            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
                return; //Make sure the WebHelper is running

            if (!spotify.Connect())
                return; //We need to call Connect before fetching infos, this will handle Auth stuff

            var status = spotify.GetStatus(); //status contains infos
            Debug.WriteLine($"Current status: {JsonConvert.SerializeObject(status)}");
            UpdateDisplay(status.Track);

            spotify.OnTrackChange += OnTrackChange;
            spotify.OnPlayStateChange += OnPlayStateChange;
            spotify.ListenForEvents = true;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        private void OnPlayStateChange(object sender, PlayStateEventArgs playStateEventArgs)
        {
            Debug.WriteLine($"PlayState changed: {JsonConvert.SerializeObject(playStateEventArgs)}");
        }

        private void OnTrackChange(object sender, TrackChangeEventArgs trackChangeEventArgs)
        {
            Debug.WriteLine($"Track changed: {JsonConvert.SerializeObject(trackChangeEventArgs)}");
            Dispatcher.BeginInvoke(new Action(() => { UpdateDisplay(trackChangeEventArgs.NewTrack); }));
        }

        private async void UpdateDisplay(Track track)
        {
            TrackTitle.Text = track.TrackResource.Name;
            TrackArtist.Text = track.ArtistResource.Name;
            TrackImage.Source = track.AlbumResource != null ? (await track.GetAlbumArtAsync(AlbumArtSize.Size160)).ToBitmapSource() : null;
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                spotify.Previous();
            }
        }

        private async void OnPlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var status = spotify.GetStatus();
                if (status.Playing)
                {
                    await spotify.Pause();
                    btn.Content = "|>";
                }
                else
                {
                    await spotify.Play();
                    btn.Content = "||";
                }
            }
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                spotify.Skip();
            }
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
