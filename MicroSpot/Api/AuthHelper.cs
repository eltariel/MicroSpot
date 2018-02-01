using System;
using System.Threading;
using System.Windows;
using MicroSpot.Settings;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace MicroSpot.Api
{
    public class AuthHelper
    {
        private readonly Configuration config;
        private readonly AutorizationCodeAuth auth;
        private readonly Timer tokenRefreshTimer;

        public AuthHelper(Configuration config)
        {
            this.config = config;

            auth = new AutorizationCodeAuth
            {
                ClientId = config.Comms.ClientId,
                RedirectUri = "http://localhost:8888",
                Scope = (Scope)0x1FFFF,
            };
            auth.OnResponseReceivedEvent += OnAuthResponseReceived;

            tokenRefreshTimer = new Timer(OnTokenRefreshTick);
        }

        public Token Token => config.Comms.AuthToken;
        public bool IsAuthorised { get; private set; }

        public bool Authorise()
        {
            if (config.Comms.AuthToken == null)
            {
                auth.StartHttpServer(8888);
                auth.DoAuth();
                return false;
            }

            RefreshToken();
            return true;
        }

        private void OnAuthResponseReceived(AutorizationCodeAuthResponse response)
        {
            var token = auth.ExchangeAuthCode(response.Code, config.Comms.ClientSecret);

            if (string.IsNullOrWhiteSpace(token.Error))
            {
                config.Comms.AuthToken = token;
                RefreshToken();
            }
            else
            {
                MessageBox.Show($"Error authenticating: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
            }

            auth.StopHttpServer();
        }

        private void OnTokenRefreshTick(object state)
        {
            RefreshToken();
        }

        private void RefreshToken()
        {
            lock (auth)
            {
                var token = auth.RefreshToken(config.Comms.AuthToken.RefreshToken, config.Comms.ClientSecret);

                if (string.IsNullOrWhiteSpace(token.Error))
                {
                    if (string.IsNullOrWhiteSpace(token.RefreshToken))
                    {
                        // Refresh token may not always be replaced.
                        token.RefreshToken = config.Comms.AuthToken.RefreshToken;
                    }

                    config.Comms.AuthToken = token;
                    config.WriteComms();

                    IsAuthorised = true;
                }
                else
                {
                    MessageBox.Show($"Error refreshing token: {token.Error}\n\n{token.ErrorDescription}", "Auth Error");
                    IsAuthorised = false;
                }
            }

            // ReSharper disable once PossibleLossOfFraction
            tokenRefreshTimer.Change(
                TimeSpan.FromSeconds(config.Comms.AuthToken.ExpiresIn / 2),
                Timeout.InfiniteTimeSpan);
        }

        public bool SetAuth(SpotifyWebAPI webApi)
        {
            if (!IsAuthorised) return false;

            if (webApi.AccessToken != Token.AccessToken)
            {
                webApi.TokenType = Token.TokenType;
                webApi.AccessToken = Token.AccessToken;
            }

            return true;
        }
    }
}