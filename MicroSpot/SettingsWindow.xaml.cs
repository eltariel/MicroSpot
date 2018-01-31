using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutoMapper;
using MicroSpot.Settings;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace MicroSpot
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private AutorizationCodeAuth auth;

        public SettingsWindow(Settings.Settings settings)
        {
            InitializeComponent();

            // Clone the settings instead of just reference
            Settings = settings.Clone();
            DataContext = Settings;
        }

        public Settings.Settings Settings { get; }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            //Create the auth object
            auth = new AutorizationCodeAuth
            {
                ClientId = Settings.Comms.ClientId,
                RedirectUri = "http://localhost:8888",
                Scope = (Scope)0x1FFFF,
            };
            auth.OnResponseReceivedEvent += OnAuthResponseReceived;
            auth.StartHttpServer(8888);
            auth.DoAuth();
        }

        private void OnAuthResponseReceived(AutorizationCodeAuthResponse response)
        {
            var token = auth.ExchangeAuthCode(response.Code, Settings.Comms.ClientSecret);

            if (string.IsNullOrWhiteSpace(token.Error))
            {
                Settings.Comms.AuthToken = token;

            }
            else
            {
                MessageBox.Show($"Error authenticating: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
            }

            //Stop the HTTP Server, done.
            auth.StopHttpServer();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Configuration.Write(Settings);
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
