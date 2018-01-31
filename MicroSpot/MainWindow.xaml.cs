using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MicroSpot.Api;
using SpotifyAPI.Local.Enums;

namespace MicroSpot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Settings.Settings settings;
        private TrackDetails currentTrack;
        private ISpotifyApi api;

        public MainWindow()
        {
            InitializeComponent();

            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                settings = Configuration.Load<Settings.Settings>();
            }
            catch (Exception ex)
            {
                settings = default;
                MessageBox.Show($"Can't parse config, generating new one instead.\n\n--- EXCEPTION ---\n{ex}");
            }

            if (settings == default(Settings.Settings))
            {
                settings = new Settings.Settings();
                WritePositionSettings();
            }

            UpdateDisplaySettings();
        }

        private void Connect()
        {
            api = SpotifyApiFactory.GetSpotifyApi(settings.Comms);
            api.Connect();

            var status = api.GetStatus();
            UpdatePlayState(status.IsPlaying);
            UpdateDisplay(status.Track);

            api.OnTrackChange += OnTrackChange;
            api.OnPlayStateChange += OnPlayStateChange;
            api.OnTrackTimeChange += OnTrackTimeChange;
        }

        private void UpdateDisplaySettings()
        {
            Top = settings.Ui.Top;
            Left = settings.Ui.Left;
            Height = settings.Ui.Height;
            Width = settings.Ui.Width;

            TrackProgress.Foreground = settings.Ui.ProgressBrush;
            TrackTitle.Foreground = settings.Ui.TrackTitleBrush;
            TrackArtist.Foreground = settings.Ui.TrackArtistBrush;
            TrackTime.Foreground = settings.Ui.TimeBrush;
            Background = settings.Ui.BackgroundBrush;

            var assetPath = $"/Assets/{(settings.Ui.DarkIcons?"Light":"Dark")}";
            RewindImage.Source = new BitmapImage(new Uri($"{assetPath}/transport.rew.png", UriKind.Relative));
            PlayImage.Source = new BitmapImage(new Uri($"{assetPath}/transport.play.png", UriKind.Relative));
            PauseImage.Source = new BitmapImage(new Uri($"{assetPath}/transport.pause.png", UriKind.Relative));
            ForwardImage.Source = new BitmapImage(new Uri($"{assetPath}/transport.ff.png", UriKind.Relative));
            SettingsImage.Source = new BitmapImage(new Uri($"{assetPath}/feature.settings.png", UriKind.Relative));
        }

        private void UpdatePlayState(bool playing)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => UpdatePlayState(playing)));
                return;
            }

            PlayImage.Visibility = playing ? Visibility.Hidden : Visibility.Visible;
            PauseImage.Visibility = playing ? Visibility.Visible : Visibility.Hidden;
        }

        private async void UpdateDisplay(TrackDetails track)
        {
            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.BeginInvoke(new Action(() => UpdateDisplay(track)));
                return;
            }

            currentTrack = track;
            TrackTitle.Text = track?.Name ?? "No track";
            TrackArtist.Text = track?.Artist ?? string.Empty;
            TrackImage.Source = (await api.GetAlbumArtAsync(AlbumArtSize.Size160)).ToBitmapSource();
            TrackProgress.Maximum = track?.Length.TotalMilliseconds ?? 0;
        }

        private void OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            UpdatePlayState(e.IsPlaying);
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

            TrackTime.Text = $@"{e.Time:m\:ss}/{currentTrack.Length:m\:ss}";
            if (e.Time < (currentTrack?.Length ?? TimeSpan.Zero))
            {
                TrackProgress.Value = (int) e.Time.TotalMilliseconds;
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            api.Previous();
        }

        private async void OnPlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (api.GetStatus().IsPlaying)
            {
                await api.Pause();
            }
            else
            {
                await api.Play();
            }
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            api.Skip();
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            var sw = new SettingsWindow(settings);
            if (sw.ShowDialog() ?? true)
            {
                settings = sw.Settings;
                Connect();
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            WritePositionSettings();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void WritePositionSettings()
        {
            settings.Ui.Top = Top;
            settings.Ui.Left = Left;
            settings.Ui.Height = Height;
            settings.Ui.Width = Width;

            Configuration.Write(settings);
        }
    }
}
