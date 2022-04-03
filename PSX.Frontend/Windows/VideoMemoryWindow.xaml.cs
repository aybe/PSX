﻿using PSX.Frontend.Core.Interface;

namespace PSX.Frontend.Windows;

public partial class VideoMemoryWindow : IVideoMemoryView
{
    public VideoMemoryWindow(ViewMemoryViewModel model)
    {
        InitializeComponent();

        DataContext = model;

        Loaded += (_, _) =>
        {
            model.IsActive = true;
        };

        Closed += (_, _) =>
        {
            model.IsActive = false;
        };
    }
}