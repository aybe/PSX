﻿using System;
using Microsoft.Win32;
using PSX.Frontend.Services;

namespace PSX.Frontend.WPF.Services;

internal sealed class FileDialogService : IFileDialogService
{
    public string? OpenFile(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(filter));

        var dialog = new OpenFileDialog
        {
            Filter = filter
        };

        return dialog.ShowDialog() is true ? dialog.FileName : null;
    }
}