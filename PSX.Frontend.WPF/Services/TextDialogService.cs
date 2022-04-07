using System;
using System.Windows;
using PSX.Frontend.Services;

namespace PSX.Frontend.WPF.Services;

internal sealed class TextDialogService : ITextDialogService
{
    public TextDialogResult Show(string caption, string message, TextDialogButton button, TextDialogImage image)
    {
        return ConvertResult(MessageBox.Show(message, caption, ConvertButton(button), ConvertImage(image)));
    }

    private static MessageBoxButton ConvertButton(TextDialogButton button)
    {
        return button switch
        {
            TextDialogButton.OK          => MessageBoxButton.OK,
            TextDialogButton.OKCancel    => MessageBoxButton.OKCancel,
            TextDialogButton.YesNo       => MessageBoxButton.YesNo,
            TextDialogButton.YesNoCancel => MessageBoxButton.YesNoCancel,
            _                        => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }

    private static MessageBoxImage ConvertImage(TextDialogImage image)
    {
        return image switch
        {
            TextDialogImage.None        => MessageBoxImage.None,
            TextDialogImage.Error       => MessageBoxImage.Error,
            TextDialogImage.Question    => MessageBoxImage.Question,
            TextDialogImage.Warning     => MessageBoxImage.Warning,
            TextDialogImage.Information => MessageBoxImage.Information,
            _                       => throw new ArgumentOutOfRangeException(nameof(image), image, null)
        };
    }

    private static TextDialogResult ConvertResult(MessageBoxResult result)
    {
        return result switch
        {
            MessageBoxResult.None   => TextDialogResult.None,
            MessageBoxResult.OK     => TextDialogResult.OK,
            MessageBoxResult.Yes    => TextDialogResult.Yes,
            MessageBoxResult.No     => TextDialogResult.No,
            MessageBoxResult.Cancel => TextDialogResult.Cancel,
            _                       => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }
}