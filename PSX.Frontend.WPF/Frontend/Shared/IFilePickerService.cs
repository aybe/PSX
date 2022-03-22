namespace PSX.Frontend.WPF.Frontend.Shared;

public interface IFilePickerService
{
    string? OpenFile(string? initialDirectory = null, string? filter = null);
}