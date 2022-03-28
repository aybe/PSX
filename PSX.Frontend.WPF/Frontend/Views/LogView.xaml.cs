using System;
using PSX.Frontend.Core.Interfaces;
using PSX.Frontend.Core.Models;

namespace PSX.Frontend.WPF.Frontend.Views;

internal sealed partial class LogView : ILogView
{
    public LogView(LogViewModel model)
    {
        InitializeComponent();
        DataContext = model ?? throw new ArgumentNullException(nameof(model));
    }
}