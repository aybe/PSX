using JetBrains.Annotations;
using Microsoft.Toolkit.Mvvm.Input;
using PSX.Frontend.WPF.Emulation.Messaging;
using PSX.Frontend.WPF.Frontend.Shared;

namespace PSX.Frontend.WPF.Frontend;

[UsedImplicitly]
internal sealed class VideoOutputModelCommands : BaseModelCommands<VideoOutputModel>
{
    public VideoOutputModelCommands(VideoOutputModel model) : base(model)
    {
    }

    public RelayCommand<BaseUpdateMessage<UpdateVideoDataMessageHandler>> UpdateVideoData { get; }

    public RelayCommand<BaseUpdateMessage<UpdateVideoSizeMessageHandler>> UpdateVideoSize { get; }
}