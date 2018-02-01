using System.Windows;
using MicroSpot.Settings;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;

namespace MicroSpot
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private AutorizationCodeAuth auth;

        public SettingsWindow(Configuration config)
        {
            InitializeComponent();

            // Clone the configuration instead of just reference
            Configuration = config.Clone();
            DataContext = Configuration;
        }

        public Configuration Configuration { get; }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            //Create the auth object
            auth = new AutorizationCodeAuth
            {
                ClientId = Configuration.Comms.ClientId,
                RedirectUri = "http://localhost:8888",
                Scope = (Scope)0x1FFFF,
            };
            auth.OnResponseReceivedEvent += OnAuthResponseReceived;
            auth.StartHttpServer(8888);
            auth.DoAuth();
        }

        private void OnAuthResponseReceived(AutorizationCodeAuthResponse response)
        {
            var token = auth.ExchangeAuthCode(response.Code, Configuration.Comms.ClientSecret);

            if (string.IsNullOrWhiteSpace(token.Error))
            {
                Configuration.Comms.AuthToken = token;
            }
            else
            {
                MessageBox.Show($"Error authenticating: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
            }

            auth.StopHttpServer();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Configuration.WriteAll();
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
