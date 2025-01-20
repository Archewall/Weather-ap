using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WeatherApp.Services
{
    public class LocalizationService
    {
        private Dictionary<string, Dictionary<string, object>> _translations;
        private string _currentLanguage;
        
        public event EventHandler? LanguageChanged;

        public LocalizationService()
        {
            _translations = new Dictionary<string, Dictionary<string, object>>();
            LoadTranslations();
            _currentLanguage = "fr"; // Langue par défaut
        }

        private void LoadTranslations()
        {
            try
            {
                var frJson = File.ReadAllText("Resources/fr.json");
                var enJson = File.ReadAllText("Resources/en.json");

                _translations["fr"] = JsonConvert.DeserializeObject<Dictionary<string, object>>(frJson)!;
                _translations["en"] = JsonConvert.DeserializeObject<Dictionary<string, object>>(enJson)!;
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_detailed.log", $"Erreur de chargement des traductions: {ex.Message}\n");
            }
        }

        public void SetLanguage(string language)
        {
            if (_translations.ContainsKey(language) && _currentLanguage != language)
            {
                _currentLanguage = language;
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GetString(string key)
        {
            try
            {
                var parts = key.Split('.');
                var current = _translations[_currentLanguage];

                foreach (var part in parts)
                {
                    if (current[part] is Dictionary<string, object> dict)
                    {
                        current = dict;
                    }
                    else
                    {
                        return current[part]?.ToString() ?? key;
                    }
                }

                return key;
            }
            catch
            {
                return key;
            }
        }

        public string CurrentLanguage => _currentLanguage;
    }
} 