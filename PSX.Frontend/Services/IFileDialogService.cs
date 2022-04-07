namespace PSX.Frontend.Services;

public interface IFileDialogService
{
    string? OpenFile(string filter);
}