
using Newtonsoft.Json;
using System;
using System.IO;

namespace DesktopFolder
{
    public class Settings
    {
        private static readonly string FilePath = "settings.json";
        public static Settings Current { get; private set; } = LoadSettings();

        public event Action? OnSettingsChanged = delegate { };

        private int _iconsPerRow = 4;
        public int IconsPerRow
        {
            get => _iconsPerRow;
            set
            {
                // Гарантируем минимум 3 иконки в строке
                _iconsPerRow = Math.Max(3, value);
                OnSettingsChanged?.Invoke();
            }
        }

        public bool ShowCollapsedName { get; set; } = true;
        public bool ShowExpandedName { get; set; } = true;

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FilePath, json);
            OnSettingsChanged?.Invoke();
        }

        private static Settings LoadSettings()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
                catch
                {
                    return new Settings();
                }
            }
            return new Settings();
        }
    }
}