using JournalPersonalApp.Data.Abstractions;

namespace JournalPersonalApp.Data.Services;

public sealed class ThemeService : IThemeService
{
    private const string ThemeKey = "app_theme_mode";

    public AppThemeMode GetTheme()
    {
        var raw = Preferences.Get(ThemeKey, nameof(AppThemeMode.Light));
        return Enum.TryParse<AppThemeMode>(raw, out var theme) ? theme : AppThemeMode.Light;
    }

    public void SetTheme(AppThemeMode theme)
    {
        Preferences.Set(ThemeKey, theme.ToString());

        // Apply immediately to MAUI
        Application.Current!.UserAppTheme = theme == AppThemeMode.Dark
            ? AppTheme.Dark
            : AppTheme.Light;
    }
}
