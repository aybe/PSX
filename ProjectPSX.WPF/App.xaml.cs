using System.Linq;
using System.Windows;

namespace ProjectPSX.WPF;

public partial class App
{
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        var window = new MainWindow();

        window.EmulatorExecutable = e.Args.FirstOrDefault();

        window.Show();
    }
}