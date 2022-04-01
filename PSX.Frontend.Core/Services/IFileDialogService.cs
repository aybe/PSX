namespace PSX.Frontend.Core.Services;

public interface IFileDialogService
{
    string? OpenFile(string filter);
}