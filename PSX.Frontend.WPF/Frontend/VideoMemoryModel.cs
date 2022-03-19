using PSX.Frontend.WPF.Frontend.Shared;

namespace PSX.Frontend.WPF.Frontend;

internal sealed class VideoMemoryModel : BaseVideoModel
{
    protected override void OnActivated()
    {
        base.OnActivated();

        IsFrameBuffer = true;
    }
}