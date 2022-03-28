namespace PSX.Frontend.Core.Services;

public interface IOpenFileService
{
    string? OpenFile(string? initialDirectory = null, string? filter = null);
}