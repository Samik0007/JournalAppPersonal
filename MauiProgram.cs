using JournalPersonalApp.Data.Abstractions;
using JournalPersonalApp.Data.Services;
using JournalPersonalApp.Data.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;

namespace JournalPersonalApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 5000;
            });

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journalAppDb.db3");
            builder.Services.AddDbContext<DBcontext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<DBservices>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IThemeService, ThemeService>();
            builder.Services.AddScoped<IPdfExportService, PdfExportService>();
            builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
