using Microsoft.Toolkit.Mvvm.ComponentModel;
using PSX.Frontend.Core.Models;

namespace PSX.Frontend.Core.ViewModels;

public sealed class ViewScreenViewModel : ObservableRecipient
{
    public ViewScreenViewModel(ViewScreenModel model)
    {
        Model = model;
    }

    private ViewScreenModel Model { get; }
}