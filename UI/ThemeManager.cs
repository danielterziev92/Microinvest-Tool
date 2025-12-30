using System;
using System.Collections.Generic;
using Microsoft.Win32;
using SQLServerManager.UI.Themes;

namespace SQLServerManager.UI
{
    public class ThemeManager
    {
        private static ThemeManager instance;
        private AppTheme currentTheme;
        
        public event EventHandler ThemeChanged;
        
        private ThemeManager()
        {
            currentTheme = GetSystemTheme();
        }
        
        public static ThemeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ThemeManager();
                }
                return instance;
            }
        }
        
        public AppTheme Current
        {
            get { return currentTheme; }
            set
            {
                if (currentTheme != value)
                {
                    currentTheme = value;
                    OnThemeChanged();
                }
            }
        }
        
        protected virtual void OnThemeChanged()
        {
            if (ThemeChanged != null)
            {
                ThemeChanged(this, EventArgs.Empty);
            }
        }
        
        public static AppTheme Theme
        {
            get { return Instance.Current; }
        }
        
        public static List<AppTheme> AvailableThemes
        {
            get
            {
                List<AppTheme> themes = new List<AppTheme>();
                themes.Add(new LightTheme());
                themes.Add(new DarkTheme());
                return themes;
            }
        }
        
        public static void SetLightTheme()
        {
            Instance.Current = new LightTheme();
        }
        
        public static void SetDarkTheme()
        {
            Instance.Current = new DarkTheme();
        }
        
        public static AppTheme GetSystemTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AppsUseLightTheme");
                        
                        if (value != null)
                        {
                            int useLightTheme = (int)value;
                            return useLightTheme == 0 ? new DarkTheme() : new LightTheme();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error reading system theme: " + ex.Message);
            }
            
            return new LightTheme();
        }
        
        public static bool IsSystemDarkMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            return (int)value == 0;
                        }
                    }
                }
            }
            catch
            {
            }
            
            return false;
        }
        
        public static void ApplySystemTheme()
        {
            Instance.Current = GetSystemTheme();
        }
        
        public static string GetSystemThemeName()
        {
            return IsSystemDarkMode() ? "Dark" : "Light";
        }
    }
}