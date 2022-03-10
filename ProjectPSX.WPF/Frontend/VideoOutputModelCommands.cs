using JetBrains.Annotations;
using Microsoft.Toolkit.Mvvm.Input;
using ProjectPSX.WPF.Emulation;
using ProjectPSX.WPF.Emulation.Messaging;
using ProjectPSX.WPF.Frontend.Shared;

namespace ProjectPSX.WPF.Frontend;

[UsedImplicitly]
internal sealed class VideoOutputModelCommands : BaseModelCommands<VideoOutputModel>
{
    public VideoOutputModelCommands(VideoOutputModel model) : base(model)
    {
    }

    public RelayCommand<BaseUpdateMessage<UpdateVideoDataMessageHandler>> UpdateVideoData { get; }

    public RelayCommand<BaseUpdateMessage<UpdateVideoSizeMessageHandler>> UpdateVideoSize { get; }
}