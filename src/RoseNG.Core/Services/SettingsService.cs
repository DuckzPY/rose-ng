using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoseNG.Core.Services
{
    public class AppSettings
    {
        [JsonPropertyName("hibpApiKey")]
        public string HibpApiKey { get; set; } = "";
    }

    // Everything is local, no telemetry: settings live in a plain JSON file
    // under the OS-standard app-data folder.
    public static class SettingsService
    {
        private static AppSettings? _cached;

        public static string AppDataDir
        {
            get
            {
                var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrWhiteSpace(baseDir))
                    baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var dir = Path.Combine(baseDir, "RoseNg");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private static string SettingsPath => Path.Combine(AppDataDir, "settings.json");

        public static AppSettings Current
        {
            get
            {
                if (_cached != null) return _cached;
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        var json = File.ReadAllText(SettingsPath);
                        _cached = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                    }
                    else
                    {
                        _cached = new AppSettings();
                    }
                }
                catch
                {
                    // Corrupt/unreadable settings file shouldn't crash the app
                    _cached = new AppSettings();
                }
                return _cached;
            }
        }

        public static void Save(AppSettings settings)
        {
            _cached = settings;
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Best-effort persistence; failing to save shouldn't crash the app
            }
        }

        public static void SetHibpApiKey(string key)
        {
            var s = Current;
            s.HibpApiKey = key ?? "";
            Save(s);
        }
    }
}
