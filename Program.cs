using Avalonia;
using System;
using System.IO;
using Avalonia.Logging;

namespace WeatherApp
{
    class Program
    {
        private static readonly string LogFile = "app.log";

        public static void Main(string[] args)
        {
            try
            {
                Log("Démarrage de l'application...");
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
                Log("Application démarrée avec succès");
            }
            catch (Exception ex)
            {
                Log($"ERREUR - {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void Log(string message)
        {
            var logMessage = $"{DateTime.Now}: {message}\n";
            File.AppendAllText(LogFile, logMessage);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
} 