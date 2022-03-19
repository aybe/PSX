using System;
using System.Windows.Forms;
using PSX.Frontend.WinForms.UI;

namespace PSX.Frontend.WinForms;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        //Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Window());
    }
}