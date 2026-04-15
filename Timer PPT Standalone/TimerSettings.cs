using System;
using System.Drawing;
using Microsoft.Win32;

namespace Timer_PPT
{
    public sealed class TimerSettings
    {
        public int DefaultSeconds { get; set; }
        public int FontSize { get; set; }
        public int FontColorArgb { get; set; }
        public bool SoundOnFinish { get; set; }
        public bool RememberPosition { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

        public static TimerSettings Default()
        {
            return new TimerSettings
            {
                DefaultSeconds = 300,
                FontSize = 48,
                FontColorArgb = Color.FromArgb(0, 200, 83).ToArgb(),
                SoundOnFinish = false,
                RememberPosition = true,
                PosX = 100,
                PosY = 100
            };
        }
    }

    public static class TimerSettingsStore
    {
        private const string BaseKey = "Software\\TimerPPT";

        public static TimerSettings Load()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(BaseKey))
                {
                    var s = TimerSettings.Default();

                    s.DefaultSeconds = ReadInt(key, "DefaultSeconds", s.DefaultSeconds);
                    s.FontSize = ReadInt(key, "FontSize", s.FontSize);
                    s.FontColorArgb = ReadInt(key, "FontColorArgb", s.FontColorArgb);
                    s.SoundOnFinish = ReadBool(key, "SoundOnFinish", s.SoundOnFinish);
                    s.RememberPosition = ReadBool(key, "RememberPosition", s.RememberPosition);
                    s.PosX = ReadInt(key, "PosX", s.PosX);
                    s.PosY = ReadInt(key, "PosY", s.PosY);

                    return s;
                }
            }
            catch
            {
                return TimerSettings.Default();
            }
        }

        public static void Save(TimerSettings settings)
        {
            if (settings == null) return;

            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(BaseKey))
                {
                    key.SetValue("DefaultSeconds", settings.DefaultSeconds, RegistryValueKind.DWord);
                    key.SetValue("FontSize", settings.FontSize, RegistryValueKind.DWord);
                    key.SetValue("FontColorArgb", settings.FontColorArgb, RegistryValueKind.DWord);
                    key.SetValue("SoundOnFinish", settings.SoundOnFinish ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("RememberPosition", settings.RememberPosition ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("PosX", settings.PosX, RegistryValueKind.DWord);
                    key.SetValue("PosY", settings.PosY, RegistryValueKind.DWord);
                }
            }
            catch
            {
            }
        }

        private static int ReadInt(RegistryKey key, string name, int fallback)
        {
            try
            {
                var val = key.GetValue(name);
                if (val == null) return fallback;
                return Convert.ToInt32(val);
            }
            catch
            {
                return fallback;
            }
        }

        private static bool ReadBool(RegistryKey key, string name, bool fallback)
        {
            return ReadInt(key, name, fallback ? 1 : 0) != 0;
        }
    }
}
