using System.Windows;

namespace Age_Of_Nothing
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new UI.MainWindow().ShowDialog();
        }
    }
}
