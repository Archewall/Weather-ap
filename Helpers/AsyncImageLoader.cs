using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using System;
using System.Net.Http;
using System.Reactive.Linq;

namespace WeatherApp.Helpers
{
    public class AsyncImageLoader : AvaloniaObject
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static readonly AttachedProperty<string?> PathProperty =
            AvaloniaProperty.RegisterAttached<AsyncImageLoader, Image, string?>(
                "Path", 
                defaultValue: null,
                inherits: false);

        static AsyncImageLoader()
        {
            PathProperty.Changed.Subscribe(args =>
            {
                if (args.Sender is Image image && !string.IsNullOrEmpty(args.NewValue.Value))
                {
                    LoadImage(image, args.NewValue.Value);
                }
            });
        }

        public static async void LoadImage(Image image, string url)
        {
            try
            {
                var stream = await _httpClient.GetStreamAsync(url);
                image.Source = new Bitmap(stream);
            }
            catch
            {
                // Gérer silencieusement les erreurs de chargement d'image
            }
        }

        public static string? GetPath(AvaloniaObject obj) => obj.GetValue(PathProperty);
        public static void SetPath(AvaloniaObject obj, string? value) => obj.SetValue(PathProperty, value);
    }
} 