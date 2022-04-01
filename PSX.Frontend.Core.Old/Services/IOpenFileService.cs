namespace PSX.Frontend.Core.Old.Services;

public interface IOpenFileService
{
    string? OpenFile(string? initialDirectory = null, string? filter = null);
}