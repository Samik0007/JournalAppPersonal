namespace JournalPersonalApp.Data.Abstractions;

public enum AppThemeMode
{
    Light,
    Dark
}

public interface IThemeService
{
    AppThemeMode GetTheme();
    void SetTheme(AppThemeMode theme);
}
