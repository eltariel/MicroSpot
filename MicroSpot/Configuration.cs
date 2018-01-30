using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MicroSpot
{
    /// <summary>
    /// Configuration handling methods.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Load a configuration from a JSON file.
        /// </summary>
        /// <param name="configPath">The path to the configuration file. Default path is config.json.</param>
        /// <returns>The configuration from the file, or default if file does not exist.</returns>
        public static T Load<T>(string configPath = "config.json")
        {
            T config = default;
            var js = JsonSerializer.Create(new JsonSerializerSettings {Formatting = Formatting.Indented,});

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
        public static void Write<T>(T config, string configPath = "config.json")
        {
            var js = JsonSerializer.Create(new JsonSerializerSettings {Formatting = Formatting.Indented,});
            using (var tw = new StreamWriter(configPath))
            using (var jtw = new JsonTextWriter(tw))
            {
                js.Serialize(jtw, config);
            }
        }
    }
}