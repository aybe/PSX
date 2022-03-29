using System;
using PSX.Frontend.Core.Modules;
using PSX.Frontend.Core.Navigation;

namespace PSX.Frontend.WPF.Views;

internal sealed partial class ViewLog : IViewLog, INavigationTarget
{
    public ViewLog(ViewModelLog model)
    {
        InitializeComponent();
        DataContext = model ?? throw new ArgumentNullException(nameof(model));
    }
}