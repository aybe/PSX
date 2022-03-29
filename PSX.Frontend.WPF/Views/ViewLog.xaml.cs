using System;
using PSX.Frontend.Core.Modules;

namespace PSX.Frontend.WPF.Views;

internal sealed partial class ViewLog : IViewLog
{
    public ViewLog(ViewModelLog model)
    {
        InitializeComponent();
        DataContext = model ?? throw new ArgumentNullException(nameof(model));
    }
}