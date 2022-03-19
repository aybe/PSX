using ProjectPSX.WPF.Frontend.Shared;

namespace ProjectPSX.WPF.Frontend;

internal sealed class VideoMemoryModel : BaseVideoModel
{
    protected override void OnActivated()
    {
        base.OnActivated();

        IsFrameBuffer = true;
    }
}