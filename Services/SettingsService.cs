using System;
using System.IO;
using Newtonsoft.Json;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class SettingsService
    {
        private const string SettingsFile = "settings.json";

        public Settings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_detailed.log", $"Erreur lors du chargement des paramètres : {ex.Message}\n");
            }

            return new Settings();
        }

        public void SaveSettings(Settings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_detailed.log", $"Erreur lors de la sauvegarde des paramètres : {ex.Message}\n");
            }
        }
    }
} 