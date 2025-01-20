using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherApp.Models;
using System.IO;

namespace WeatherApp.Services
{
    public class WeatherService
    {
        private const string API_KEY = "1b47f80a7e337561c2427fee86f525c8";
        private const string BASE_URL = "https://api.openweathermap.org/data/2.5";
        private readonly HttpClient _client = new HttpClient();

        private void LogToFile(string message)
        {
            try
            {
                File.AppendAllText("weather.log", $"{DateTime.Now}: {message}\n");
            }
            catch
            {
                // Ignorer les erreurs de logging
            }
        }

        public async Task<Weather?> GetCurrentWeather(string city)
        {
            try
            {
                File.AppendAllText("app_detailed.log", $"Demande météo pour {city}\n");
                var response = await _client.GetAsync(
                    $"{BASE_URL}/weather?q={city}&appid={API_KEY}&units=metric&lang=fr");
                
                File.AppendAllText("app_detailed.log", $"Réponse reçue: {response.StatusCode}\n");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<WeatherResponse>(json);
                    
                    if (data?.Weather?.Count > 0)
                    {
                        return new Weather
                        {
                            CityName = data.Name ?? "",
                            Latitude = data.Coord?.Lat ?? 0,
                            Longitude = data.Coord?.Lon ?? 0,
                            Temperature = data.Main?.Temp ?? 0,
                            Description = data.Weather[0].Description ?? "",
                            Humidity = data.Main?.Humidity ?? 0,
                            IconUrl = $"https://openweathermap.org/img/w/{data.Weather[0].Icon}.png"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("app_detailed.log", $"Erreur météo: {ex.Message}\n");
            }
            
            return null;
        }

        public async Task<List<Weather>> GetForecast(string city)
        {
            try
            {
                LogToFile($"Demande de prévisions pour {city}");
                var url = $"{BASE_URL}/forecast?q={city}&appid={API_KEY}&units=metric&lang=fr";
                LogToFile($"URL de l'API: {url}");
                
                var response = await _client.GetAsync(url);
                LogToFile($"Code de réponse: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    LogToFile($"Réponse brute: {json}");
                    var data = JsonConvert.DeserializeObject<ForecastResponse>(json);
                    
                    if (data?.List != null)
                    {
                        var forecast = data.List
                            .Where(f => f.DtTxt.Hour == 12)
                            .Take(5)
                            .Select(f => new Weather
                            {
                                Date = f.DtTxt,
                                Temperature = Math.Round(f.Main?.Temp ?? 0, 1),
                                Description = f.Weather?[0]?.Description ?? "",
                                Humidity = f.Main?.Humidity ?? 0,
                                IconUrl = f.Weather?[0]?.Icon != null 
                                    ? $"https://openweathermap.org/img/w/{f.Weather[0].Icon}.png"
                                    : null
                            })
                            .ToList();

                        LogToFile($"Prévisions filtrées : {forecast.Count} jours");
                        return forecast;
                    }
                    else
                    {
                        LogToFile("Données de prévision nulles ou vides");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogToFile($"Erreur API: {response.StatusCode}, Contenu: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Erreur lors de la récupération des prévisions: {ex.Message}\n{ex.StackTrace}");
            }
            
            return new List<Weather>();
        }
    }
} 