using Microsoft.Win32;
using PSX.Frontend.Core.Old.Services;

namespace PSX.Frontend.WPF.Services;

internal sealed class OpenFileService : IOpenFileService
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