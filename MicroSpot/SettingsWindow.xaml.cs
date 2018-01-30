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
using SpotifyAPI.Web;
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

        public SettingsWindow(Settings settings)
        {
            Settings = settings;

            // TODO: Copy settings into datacontext rather than just reference, so that we can do ok/cancel nicely
            DataContext = settings;

            InitializeComponent();
        }


        public Settings Settings { get; }
        public SpotifyWebAPI SpotifyWeb { get; private set; }

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

                SpotifyWeb = new SpotifyWebAPI
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken
                };
            }
            else
            {
                MessageBox.Show($"Error authenticating: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
            }

            //Stop the HTTP Server, done.
            auth.StopHttpServer();
        }
    }
}
