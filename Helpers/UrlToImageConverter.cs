using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace WeatherApp.Helpers
{
    public class UrlToImageConverter : IValueConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly UrlToImageConverter _instance = new();
        public static UrlToImageConverter Instance => _instance;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrEmpty(url))
            {
                try
                {
                    var task = Task.Run(async () =>
                    {
                        var bytes = await _httpClient.GetByteArrayAsync(url);
                        using var stream = new MemoryStream(bytes);
                        return new Bitmap(stream);
                    });

                    return task.Result;
                }
                catch (Exception ex)
                {
                    File.AppendAllText("app_detailed.log", $"Erreur de conversion d'image: {ex.Message}\n");
                    return null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 