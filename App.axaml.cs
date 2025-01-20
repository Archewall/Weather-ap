using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;

namespace WeatherApp
{
    public class App : Application
    {
        private void Log(string message)
        {
            try
            {
                File.AppendAllText("app_detailed.log", $"{DateTime.Now}: {message}\n");
            }
            catch { }
        }

        public override void Initialize()
        {
            try
            {
                Log("Début de l'initialisation");
                AvaloniaXamlLoader.Load(this);
                Log("Initialisation terminée");
            }
            catch (Exception ex)
            {
                Log($"Erreur d'initialisation: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            try
            {
                Log("Début de la configuration de la fenêtre");
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = new MainWindow();
                    Log("Fenêtre principale créée");
                }
                base.OnFrameworkInitializationCompleted();
                Log("Configuration terminée");
            }
            catch (Exception ex)
            {
                Log($"Erreur de configuration: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
} 