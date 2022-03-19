using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PSX.Frontend.WPF.Frontend.Shared;

public abstract class BaseModel : ObservableRecipient
{
}

public abstract class BaseModel<T> : BaseModel where T : BaseModelCommands
{
    protected BaseModel()
    {
        Commands = (Activator.CreateInstance(typeof(T), this) as T)!;
    }

    public T Commands { get; }
}