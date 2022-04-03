using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PSX.Frontend.Core.Interface;

public sealed class ViewMemoryViewModel : ObservableRecipient
{
    public ViewMemoryViewModel(ViewMemoryModel model)
    {
        Model = model;
    }

    private ViewMemoryModel Model { get; }
}