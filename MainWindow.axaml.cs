using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using WeatherApp.Services;
using WeatherApp.Models;
using Avalonia.Media.Imaging;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private readonly WeatherService _weatherService;
        private readonly SettingsService _settingsService;
        private readonly HttpClient _httpClient;
        private readonly List<string> _searchHistory;
        private const int MaxHistoryItems = 5;
        
        public MainWindow()
        {
            try
            {
                File.AppendAllText("app_detailed.log", "Début de l'initialisation de MainWindow\n");
                
                InitializeComponent();
                
                _httpClient = new HttpClient();
                _weatherService = new WeatherService();
                _settingsService = new SettingsService();
                _searchHistory = new List<string>();
                
                File.AppendAllText("app_detailed.log", "Services initialisés\n");
                
                LoadSettings();
                LoadSearchHistory();
                File.AppendAllText("app_detailed.log", "Paramètres chargés\n");
                
                this.Loaded += async (s, e) =>
                {
                    try
                    {
                        File.AppendAllText("app_detailed.log", "Fenêtre chargée, début de la recherche\n");
                        var settings = _settingsService.LoadSettings();
                        SearchBox.Text = !string.IsNullOrEmpty(settings.DefaultCity) ? settings.DefaultCity : "Paris";
                        await Task.Delay(1000);
                        await LoadInitialWeather();
                        File.AppendAllText("app_detailed.log", "Recherche initiale terminée\n");
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText("app_detailed.log", $"Erreur lors du chargement initial: {ex.Message}\n{ex.StackTrace}\n");
                    }
                };
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_detailed.log", $"Erreur dans MainWindow: {ex.Message}\n{ex.StackTrace}\n");
                throw;
            }
        }

        private void LoadSearchHistory()
        {
            var settings = _settingsService.LoadSettings();
            if (!string.IsNullOrEmpty(settings.SearchHistory))
            {
                _searchHistory.Clear();
                _searchHistory.AddRange(settings.SearchHistory.Split(','));
                UpdateSearchHistoryComboBox();
            }
        }

        private void UpdateSearchHistoryComboBox()
        {
            var items = SearchHistory.Items;
            items.Clear();
            foreach (var city in _searchHistory)
            {
                items.Add(city);
            }
        }

        private void AddToHistory(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return;

            // Supprimer si existe déjà
            _searchHistory.Remove(city);
            
            // Ajouter au début
            _searchHistory.Insert(0, city);
            
            // Garder seulement les 5 dernières recherches
            while (_searchHistory.Count > MaxHistoryItems)
            {
                _searchHistory.RemoveAt(_searchHistory.Count - 1);
            }
            
            // Mettre à jour la ComboBox et sauvegarder
            UpdateSearchHistoryComboBox();
            _settingsService.SaveSettings(new Settings
            {
                DefaultCity = DefaultCityBox.Text,
                Units = "metric",
                SearchHistory = string.Join(",", _searchHistory)
            });
        }

        private void OnHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchHistory.SelectedItem is string selectedCity)
            {
                SearchBox.Text = selectedCity;
                OnSearchClick(sender, null);
            }
        }

        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();
            DefaultCityBox.Text = settings.DefaultCity;
            TemperatureUnit.SelectedIndex = settings.UseFahrenheit ? 1 : 0;
        }

        private void OnSaveSettings(object sender, RoutedEventArgs e)
        {
            _settingsService.SaveSettings(new Settings
            {
                DefaultCity = DefaultCityBox.Text,
                Units = "metric",
                SearchHistory = string.Join(",", _searchHistory),
                UseFahrenheit = TemperatureUnit.SelectedIndex == 1
            });
            
            // Mettre à jour immédiatement la météo avec la nouvelle ville par défaut
            if (!string.IsNullOrEmpty(DefaultCityBox.Text))
            {
                SearchBox.Text = DefaultCityBox.Text;
                OnSearchClick(sender, e);
            }
        }

        private void OnClearHistory(object sender, RoutedEventArgs e)
        {
            _searchHistory.Clear();
            UpdateSearchHistoryComboBox();
            _settingsService.SaveSettings(new Settings
            {
                DefaultCity = DefaultCityBox.Text,
                Units = "metric",
                SearchHistory = "",
                UseFahrenheit = TemperatureUnit.SelectedIndex == 1
            });
        }

        private string FormatTemperature(double temperature)
        {
            if (TemperatureUnit.SelectedIndex == 1) // Fahrenheit
            {
                temperature = (temperature * 9/5) + 32;
            }
            return $"{temperature:F1}°{(TemperatureUnit.SelectedIndex == 1 ? "F" : "C")}";
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text)) return;
            
            var weather = await _weatherService.GetCurrentWeather(SearchBox.Text);
            
            if (weather != null)
            {
                ErrorMessage.IsVisible = false;
                CityName.Text = weather.CityName;
                Coordinates.Text = $"{weather.Latitude}, {weather.Longitude}";
                Temperature.Text = FormatTemperature(weather.Temperature);
                WeatherDescription.Text = weather.Description;
                Humidity.Text = $"{weather.Humidity}%";
                
                // Mettre à jour l'icône météo
                if (!string.IsNullOrEmpty(weather.IconUrl))
                {
                    try
                    {
                        var bitmap = await _weatherService.GetWeatherIcon(weather.IconUrl);
                        CurrentWeatherIcon.Source = bitmap;
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText("app_detailed.log", $"Erreur lors du chargement de l'icône : {ex.Message}\n");
                    }
                }
                
                AddToHistory(weather.CityName);
                await LoadForecast(SearchBox.Text);
            }
            else
            {
                ErrorMessage.IsVisible = true;
                CityName.Text = "";
                Coordinates.Text = "";
                Temperature.Text = "";
                WeatherDescription.Text = "";
                Humidity.Text = "";
                CurrentWeatherIcon.Source = null;
                ForecastList.ItemsSource = null;
            }
        }

        private async Task LoadInitialWeather()
        {
            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                var weather = await _weatherService.GetCurrentWeather(SearchBox.Text);
                if (weather != null)
                {
                    CityName.Text = weather.CityName;
                    Coordinates.Text = $"{weather.Latitude}, {weather.Longitude}";
                    Temperature.Text = FormatTemperature(weather.Temperature);
                    WeatherDescription.Text = weather.Description;
                    Humidity.Text = $"{weather.Humidity}%";
                    
                    // Mettre à jour l'icône météo
                    if (!string.IsNullOrEmpty(weather.IconUrl))
                    {
                        try
                        {
                            var bitmap = await _weatherService.GetWeatherIcon(weather.IconUrl);
                            CurrentWeatherIcon.Source = bitmap;
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText("app_detailed.log", $"Erreur lors du chargement de l'icône : {ex.Message}\n");
                        }
                    }
                    
                    await LoadForecast(SearchBox.Text);
                }
            }
        }

        private async Task LoadForecast(string city)
        {
            try
            {
                File.AppendAllText("app_detailed.log", $"Chargement des prévisions pour {city}\n");
                var forecast = await _weatherService.GetForecast(city);
                File.AppendAllText("app_detailed.log", $"Nombre de prévisions reçues : {forecast.Count}\n");
                
                if (forecast.Count > 0)
                {
                    // Convertir les températures si nécessaire
                    foreach (var item in forecast)
                    {
                        if (TemperatureUnit.SelectedIndex == 1) // Fahrenheit
                        {
                            item.Temperature = (item.Temperature * 9/5) + 32;
                            item.Description = $"{item.Description} ({item.Temperature:F1}°F)";
                        }
                        else
                        {
                            item.Description = $"{item.Description} ({item.Temperature:F1}°C)";
                        }
                    }
                    
                    ForecastList.ItemsSource = null;
                    ForecastList.ItemsSource = forecast;
                    File.AppendAllText("app_detailed.log", "Prévisions affichées dans la liste\n");
                }
                else
                {
                    File.AppendAllText("app_detailed.log", "Aucune prévision reçue\n");
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_detailed.log", $"Erreur lors du chargement des prévisions : {ex.Message}\n{ex.StackTrace}\n");
            }
        }

        private void OnTemperatureUnitChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                OnSearchClick(sender, null);
            }
        }
    }
} 