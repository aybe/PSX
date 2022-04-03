using Microsoft.Toolkit.Mvvm.ComponentModel;
using PSX.Frontend.Core.Models;

namespace PSX.Frontend.Core.ViewModels;

public sealed class ViewMemoryViewModel : ObservableRecipient
{
    public ViewMemoryViewModel(ViewMemoryModel model)
    {
        Model = model;
    }

    private ViewMemoryModel Model { get; }
}