using DevExpress.Maui;
using DevExpress.Maui.Core;

namespace CommonCode.Helpers
{
    public static class SetThemeColor
    {
        public static void SetAppThemeColor()
        {
            // Default theme color
            Theme defaultTheme = new Theme(Color.Parse("#FF323943"));

            // Retrieve preferences
            bool isCustomColorTheme = Preferences.Default.Get("isCustomColorTheme", false);
            string customColor = Preferences.Default.Get("CustomColorTheme", "");
            string themeColor = Preferences.Default.Get("themeColor", "");

            Theme theme = defaultTheme;

            if (isCustomColorTheme)
            {
                // Attempt to use the custom color
                if (Color.TryParse(customColor, out Color parsedCustomColor))
                {
                    theme = new Theme(parsedCustomColor);
                }
                else if (Enum.TryParse(themeColor, out ThemeSeedColor parsedThemeSeedColor))
                {
                    theme = new Theme(parsedThemeSeedColor);
                }
            }
            else
            {
                // Attempt to use the saved theme color
                if (Enum.TryParse(themeColor, out ThemeSeedColor parsedThemeSeedColor))
                {
                    theme = new Theme(parsedThemeSeedColor);
                }
            
            }

            // Apply the theme
            ThemeManager.UseAndroidSystemColor = false;
            ThemeManager.ApplyThemeToSystemBars = true;
            ThemeManager.Theme = theme;
        }
    }
}
