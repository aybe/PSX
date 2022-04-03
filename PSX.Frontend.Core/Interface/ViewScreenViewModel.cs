using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PSX.Frontend.Core.Interface;

public sealed class ViewScreenViewModel : ObservableRecipient
{
    public ViewScreenViewModel(ViewScreenModel model)
    {
        Model = model;
    }

    private ViewScreenModel Model { get; }
}