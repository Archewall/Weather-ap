using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WeatherApp.Models
{
    public class Weather
    {
        public string? CityName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Temperature { get; set; }
        public string? Description { get; set; }
        public int Humidity { get; set; }
        public string? IconUrl { get; set; }
        public DateTime Date { get; set; }
    }

    public class WeatherResponse
    {
        public string? Name { get; set; }
        public Coord? Coord { get; set; }
        public MainInfo? Main { get; set; }
        public List<WeatherInfo>? Weather { get; set; }
    }

    public class ForecastResponse
    {
        public List<ForecastItem>? List { get; set; }
    }

    public class ForecastItem
    {
        public MainInfo? Main { get; set; }
        public List<WeatherInfo>? Weather { get; set; }
        [JsonProperty("dt_txt")]
        public DateTime DtTxt { get; set; }
    }

    public class Coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class MainInfo
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
    }

    public class WeatherInfo
    {
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }
} 