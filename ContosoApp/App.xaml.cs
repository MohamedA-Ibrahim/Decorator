using Contoso.App.ViewModels;
using Contoso.App.Views;
using Decorator.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;
using Windows.Globalization;

namespace Contoso.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets main App Window
        /// </summary>
        public static Window Window { get { return m_window; } }
        private static Window m_window;

        /// <summary>
        /// Gets the app-wide MainViewModel singleton instance.
        /// </summary>
        public static MainViewModel ViewModel { get; } = new MainViewModel();

        /// <summary>
        /// Pipeline for interacting with backend service or database.
        /// </summary>
        public static IRepository Repository { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() { 

            InitializeComponent();
            App.Current.RequestedTheme = ApplicationTheme.Light;

        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.
        /// </summary>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            m_window = new MainWindow();

            UseSqlite();

            // Prepare the app shell and window content.
            AppShell shell = m_window.Content as AppShell ?? new AppShell();
            shell.Language = ApplicationLanguages.Languages[0];
            m_window.Content = shell;

            if (shell.AppFrame.Content == null)
            {
                // When the navigation stack isn't restored, navigate to the first page
                // suppressing the initial entrance animation.
                shell.AppFrame.Navigate(typeof(ProductListPage), null,
                    new SuppressNavigationTransitionInfo());
            }

            m_window.Activate();
        }

        /// <summary>
        /// Configures the app to use the Sqlite data source. If no existing Sqlite database exists, 
        /// loads a demo database filled with fake data so the app has content.
        /// </summary>
        public static void UseSqlite()
        {
            //Get the appdata directory and create it if not exists
            string localAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Decorator";
            Directory.CreateDirectory(localAppDataDirectory);

            string localDatabasePath = localAppDataDirectory + @"\Decorator.db";
            string demoDatabasePath = AppDomain.CurrentDomain.BaseDirectory + @"\Assets\Decorator.db";

            if (!File.Exists(localDatabasePath))
            {
                File.Copy(demoDatabasePath, localDatabasePath);
            }
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite("Data Source=" + localDatabasePath);
            Repository = new Repository(dbOptions);
        }

    }
}