using JetBrains.Annotations;

namespace ProjectPSX.WPF.Frontend.Shared;

[UsedImplicitly]
internal class BaseVideoModelCommands : BaseModelCommands<BaseVideoModel>
{
    public BaseVideoModelCommands(BaseVideoModel model) : base(model)
    {
    }
}