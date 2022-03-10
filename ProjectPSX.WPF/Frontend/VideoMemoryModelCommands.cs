using JetBrains.Annotations;
using Microsoft.Toolkit.Mvvm.Input;
using ProjectPSX.WPF.Emulation;
using ProjectPSX.WPF.Emulation.Messaging;
using ProjectPSX.WPF.Frontend.Shared;

namespace ProjectPSX.WPF.Frontend;

[UsedImplicitly]
internal sealed class VideoMemoryModelCommands : BaseModelCommands<VideoMemoryModel>
{
    public VideoMemoryModelCommands(VideoMemoryModel model) : base(model)
    {
    }

    public RelayCommand<BaseUpdateMessage<UpdateVideoDataMessageHandler>> UpdateVideoData { get; }

    public RelayCommand<BaseUpdateMessage<UpdateVideoSizeMessageHandler>> UpdateVideoSize { get; }
}