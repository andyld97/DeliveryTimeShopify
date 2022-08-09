using System;
using System.Threading;
using System.Windows;

namespace DeliveryTimeShopify
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Mutex AppMutex = new Mutex(false, "06f86505-ab66-4d1f-82cc-10b4c4cb5392");

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!AppMutex.WaitOne(TimeSpan.FromSeconds(1), false))
            {
                MessageBox.Show("Das Programm läuft bereits!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
}