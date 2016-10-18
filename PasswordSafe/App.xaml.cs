using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = $"An unhandled exception occurred: {e.Exception.Message}";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            File.AppendAllText("log.txt", $"{DateTime.Now} - {errorMessage}\r\n");
            e.Handled = true;
            Current.Shutdown();
        }
    }
}