﻿using System;
using PSX.Frontend.Interface;

namespace PSX.Frontend.WPF.Windows;

public partial class VideoScreenWindow : IVideoScreenView
{
    public VideoScreenWindow(VideoViewModel model)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));

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