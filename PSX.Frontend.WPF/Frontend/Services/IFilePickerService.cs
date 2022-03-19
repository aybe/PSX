namespace ProjectPSX.WPF.Frontend.Services;

public interface IFilePickerService
{
    string? OpenFile(string? initialDirectory = null, string? filter = null);
}