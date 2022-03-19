using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace PSX.Frontend.WPF.Frontend.Shared;

public abstract class BaseModelCommands : ObservableRecipient
{
}

public abstract class BaseModelCommands<T> : BaseModelCommands where T : BaseModel
{
    protected BaseModelCommands(T model)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
    }

    protected T Model { get; }
}