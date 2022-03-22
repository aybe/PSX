using Microsoft.Win32;

namespace PSX.Frontend.WPF.Frontend.Services;

internal class FilePickerServiceWindows : IFilePickerService
{
    public string? OpenFile(string? initialDirectory = null, string? filter = null)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = initialDirectory,
            Filter           = filter
        };

        if (dialog.ShowDialog() is not true)
            return null;

        return dialog.FileName;
    }
}