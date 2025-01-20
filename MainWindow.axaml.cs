using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;
using WeatherApp.Services;
using WeatherApp.Models;
using Avalonia.Media.Imaging;
using System.Net.Http;
using System.IO;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private readonly WeatherService _weatherService;
        private readonly SettingsService _settingsService;
        private readonly HttpClient _httpClient;
        
        public MainWindow()
        {
            try
            {
                File.AppendAllText("app_detailed.log", "Début de l'initialisation de MainWindow\n");
                
                InitializeComponent();
                
                _httpClient = new HttpClient();
                _weatherService = new WeatherService();
                _settingsService = new SettingsService();
                
                File.AppendAllText("app_detailed.log", "Services initialisés\n");
                
                LoadSettings();
                File.AppendAllText("app_detailed.log", "Paramètres chargés\n");
                
                // Déplacer le chargement initial dans un événement Loaded
                this.Loaded += async (s, e) =>
                {
                    try
                    {
                        File.AppendAllText("app_detailed.log", "Fenêtre chargée, début de la recherche\n");
                        SearchBox.Text = "Paris";
                        await Task.Delay(1000); // Attendre 1 seconde
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

        private async Task LoadInitialWeather()
        {
            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                var weather = await _weatherService.GetCurrentWeather(SearchBox.Text);
                if (weather != null)
                {
                    CityName.Text = weather.CityName;
                    Coordinates.Text = $"{weather.Latitude}, {weather.Longitude}";
                    Temperature.Text = $"{weather.Temperature}°C";
                    WeatherDescription.Text = weather.Description;
                    Humidity.Text = $"{weather.Humidity}%";
                    await LoadForecast(SearchBox.Text);
                }
            }
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Recherche pour la ville : {SearchBox.Text}"); // Log de recherche
            
            if (string.IsNullOrEmpty(SearchBox.Text)) return;
            
            var weather = await _weatherService.GetCurrentWeather(SearchBox.Text);
            
            if (weather != null)
            {
                Console.WriteLine($"Météo trouvée : {weather.Temperature}°C"); // Log des résultats
                CityName.Text = weather.CityName;
                Coordinates.Text = $"{weather.Latitude}, {weather.Longitude}";
                Temperature.Text = $"{weather.Temperature}°C";
                WeatherDescription.Text = weather.Description;
                Humidity.Text = $"{weather.Humidity}%";
                if (!string.IsNullOrEmpty(weather.IconUrl))
                {
                    try 
                    {
                        var uri = new Uri(weather.IconUrl);
                        var stream = await _httpClient.GetStreamAsync(uri);
                        WeatherIcon.Source = new Bitmap(stream);
                    }
                    catch (Exception ex)
                    {
                        // Gérer l'erreur silencieusement pour éviter le crash
                        Console.WriteLine($"Erreur lors du chargement de l'image : {ex.Message}");
                    }
                }
                
                await LoadForecast(SearchBox.Text);
            }
            else
            {
                Console.WriteLine("Aucune météo trouvée"); // Log d'erreur
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
                    ForecastList.ItemsSource = null; // Reset la source
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

        private void LoadSettings()
        {
            var settings = _settingsService.LoadSettings();
            DefaultCity.Text = settings.DefaultCity;
            LanguageSelector.SelectedIndex = settings.Language == "fr" ? 0 : 1;
        }

        private void OnSaveSettings(object sender, RoutedEventArgs e)
        {
            _settingsService.SaveSettings(new Settings
            {
                DefaultCity = DefaultCity.Text,
                Language = LanguageSelector.SelectedIndex == 0 ? "fr" : "en",
                Units = "metric"
            });
        }
    }
} 