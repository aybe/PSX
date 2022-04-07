namespace PSX.Frontend.Services.Dialog;

public interface IDialogService
{
    DialogResult Show(string caption, string message, DialogButton button, DialogImage image = DialogImage.None);
}