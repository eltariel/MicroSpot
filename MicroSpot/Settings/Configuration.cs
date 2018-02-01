using System.IO;
using AutoMapper;
using Newtonsoft.Json;
using SpotifyAPI.Web.Models;

namespace MicroSpot.Settings
{
    public class Configuration
    {
        private const string COMMS_FILENAME = "config.comms.json";
        private const string UI_FILENAME = "config.ui.json";

        public Configuration()
        {
            Ui = new UiSettings();
            Comms = new CommsSettings();
        }

        public UiSettings Ui { get; set; }
        public CommsSettings Comms { get; set; }

        public bool Valid => Ui != null && Comms != null;

        public Configuration Clone()
        {
            var mapper = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<Configuration, Configuration>();
                    cfg.CreateMap<UiSettings, UiSettings>();
                    cfg.CreateMap<CommsSettings, CommsSettings>();
                    cfg.CreateMap<Token, Token>();
                }).CreateMapper();

            return mapper.Map<Configuration>(this);
        }

        public void WriteAll()
        {
            WriteComms();
            WriteUi();
        }

        public void WriteUi()
        {
            Write(Ui, UI_FILENAME);
        }

        public void WriteComms()
        {
            Write(Comms, COMMS_FILENAME);
        }

        public static Configuration Load()
        {
            return new Configuration
            {
                Comms = Load<CommsSettings>(COMMS_FILENAME),
                Ui = Load<UiSettings>(UI_FILENAME)
            };
        }

        /// <summary>
        /// Load a configuration from a JSON file.
        /// </summary>
        /// <param name="configPath">The path to the configuration file. Default path is config.json.</param>
        /// <returns>The configuration from the file, or default if file does not exist.</returns>
        private static T Load<T>(string configPath = "config.json")
        {
            T config = default;
            var js = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented, });

            if (File.Exists(configPath))
            {
                using (var sr = new StreamReader(configPath))
                using (var jr = new JsonTextReader(sr))
                {
                    config = js.Deserialize<T>(jr);
                }
            }

            return config;
        }

        /// <summary>
        /// Write a configuration to a JSON file.
        /// </summary>
        /// <param name="config">Configuration to write.</param>
        /// <param name="configPath">The path to the configuration file. Default path is config.json.</param>
        private static void Write<T>(T config, string configPath = "config.json")
        {
            var js = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented, });
            using (var tw = new StreamWriter(configPath))
            using (var jtw = new JsonTextWriter(tw))
            {
                js.Serialize(jtw, config);
            }
        }

    }
}