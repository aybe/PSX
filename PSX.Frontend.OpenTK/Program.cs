using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace ProjectPSX.OpenTK;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        var settings = new GameWindowSettings();
        settings.RenderFrequency = 60;
        settings.UpdateFrequency = 60;
        var nativeWindow = new NativeWindowSettings();
        nativeWindow.API     = ContextAPI.OpenGL;
        nativeWindow.Size    = new Vector2i(1024, 512);
        nativeWindow.Title   = "ProjectPSX";
        nativeWindow.Profile = ContextProfile.Compatability;

        var window = new Window(settings, nativeWindow);
        window.VSync = VSyncMode.On;

        if (Util.Storage.TryGetExecutable(args, out var path))
        {
            window.SetExecutable(path);
        }

        window.Run();
    }
}