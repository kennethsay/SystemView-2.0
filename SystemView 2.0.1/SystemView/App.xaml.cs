using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace SystemView
{
     /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        enum Enabled
        {
            NO,
            YES
        };

        private const int MINIMUM_SPLASH_TIME = 4000; // Miliseconds 
        private const int SPLASH_FADE_TIME = 500;     // Miliseconds 

        protected override void OnStartup(StartupEventArgs e)
        {
            // Change this to enable or disable the SplashScreen 
            Enabled MySplashScreen = Enabled.YES;

            // Users are not yet authenticated
            ContentDisplays.EmployeeLogin.UserAuthenticated = false;

            if (MySplashScreen == Enabled.YES)
            {
                // Step 1 - Load the splash screen 
                SplashScreen splash = new SplashScreen();
                splash.Show();

                // Step 2 - Start a stop watch 
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // Step 3 - Load your windows but don't show it yet 
                base.OnStartup(e);
                MainWindow main = new MainWindow();

                // Step 4 - Make sure that the splash screen lasts at least two seconds 
                timer.Stop();
                int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
                if (remainingTimeToShowSplash > 0)
                System.Threading.Thread.Sleep(remainingTimeToShowSplash);

                // Step 5 - show the page 
                splash.Close();
                main.Show();
            }
            else
            {
                MainWindow main = new MainWindow();
                main.Show();
            }
        }
    }    
}
