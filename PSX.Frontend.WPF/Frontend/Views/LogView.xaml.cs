using System;

namespace PSX.Frontend.WPF.Frontend.Views;

internal sealed partial class LogView : ILogView
{
    public LogView(ILogViewModel model)
    {
        InitializeComponent();
        DataContext = model ?? throw new ArgumentNullException(nameof(model));
    }
}