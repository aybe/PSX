namespace PSX.Frontend.Services;

public interface ITextDialogService
{
    TextDialogResult Show(string caption, string message, TextDialogButton button, TextDialogImage image = TextDialogImage.None);
}