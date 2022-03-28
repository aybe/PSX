namespace PSX.Frontend.Core.Services;

public interface IFilePickerService
{
    string? OpenFile(string? initialDirectory = null, string? filter = null);
}