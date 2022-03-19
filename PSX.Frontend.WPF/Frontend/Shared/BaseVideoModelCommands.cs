using JetBrains.Annotations;

namespace PSX.Frontend.WPF.Frontend.Shared;

[UsedImplicitly]
internal class BaseVideoModelCommands : BaseModelCommands<BaseVideoModel>
{
    public BaseVideoModelCommands(BaseVideoModel model) : base(model)
    {
    }
}