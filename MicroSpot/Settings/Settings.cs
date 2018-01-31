using AutoMapper;
using SpotifyAPI.Web.Models;

namespace MicroSpot.Settings
{
    public class Settings
    {
        public Settings()
        {
            Ui = new UiSettings();
            Comms = new CommsSettings();
        }

        public UiSettings Ui { get; set; }
        public CommsSettings Comms { get; set; }

        public Settings Clone()
        {
            var mapper = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<Settings, Settings>();
                    cfg.CreateMap<UiSettings, UiSettings>();
                    cfg.CreateMap<CommsSettings, CommsSettings>();
                    cfg.CreateMap<Token, Token>();
                }).CreateMapper();

            return mapper.Map<Settings>(this);
        }
    }
}