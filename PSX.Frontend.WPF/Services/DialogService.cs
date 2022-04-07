using System;
using System.Windows;
using PSX.Frontend.Services.Dialog;

namespace PSX.Frontend.WPF.Services;

internal sealed class DialogService : IDialogService
{
    public DialogResult Show(string caption, string message, DialogButton button, DialogImage image)
    {
        return ConvertResult(MessageBox.Show(message, caption, ConvertButton(button), ConvertImage(image)));
    }

    private static MessageBoxButton ConvertButton(DialogButton button)
    {
        return button switch
        {
            DialogButton.OK          => MessageBoxButton.OK,
            DialogButton.OKCancel    => MessageBoxButton.OKCancel,
            DialogButton.YesNo       => MessageBoxButton.YesNo,
            DialogButton.YesNoCancel => MessageBoxButton.YesNoCancel,
            _                        => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }

    private static MessageBoxImage ConvertImage(DialogImage image)
    {
        return image switch
        {
            DialogImage.None        => MessageBoxImage.None,
            DialogImage.Error       => MessageBoxImage.Error,
            DialogImage.Question    => MessageBoxImage.Question,
            DialogImage.Warning     => MessageBoxImage.Warning,
            DialogImage.Information => MessageBoxImage.Information,
            _                       => throw new ArgumentOutOfRangeException(nameof(image), image, null)
        };
    }

    private static DialogResult ConvertResult(MessageBoxResult result)
    {
        return result switch
        {
            MessageBoxResult.None   => DialogResult.None,
            MessageBoxResult.OK     => DialogResult.OK,
            MessageBoxResult.Yes    => DialogResult.Yes,
            MessageBoxResult.No     => DialogResult.No,
            MessageBoxResult.Cancel => DialogResult.Cancel,
            _                       => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }
}