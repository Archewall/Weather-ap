using System;
using System.IO;
using System.Text.Json;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "WeatherApp");
            _settingsPath = Path.Combine(appFolder, "settings.json");
        }

        public Settings LoadSettings()
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<Settings>(json);
                return settings ?? CreateDefaultSettings();
            }

            return CreateDefaultSettings();
        }

        private Settings CreateDefaultSettings()
        {
            return new Settings
            {
                DefaultCity = "Paris",
                Language = "fr",
                Units = "metric"
            };
        }

        public void SaveSettings(Settings settings)
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsPath, json);
        }
    }
} 