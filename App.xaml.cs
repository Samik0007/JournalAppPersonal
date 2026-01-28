using JournalPersonalApp.Data.Services;

namespace JournalPersonalApp
{
    public partial class App : Application
    {
        public App(DBservices databaseService)
        {
            InitializeComponent();

            // Initialize database with seed data
            Task.Run(async () =>
            {
                await databaseService.InitializeAsync();
            }).Wait();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "JournalPersonalApp" };
        }
    }
}
