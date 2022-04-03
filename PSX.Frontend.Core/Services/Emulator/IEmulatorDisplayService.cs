using PSX.Core.Interfaces;

namespace PSX.Frontend.Core.Services.Emulator;

public interface IEmulatorDisplayService : IHostWindow // TODO rename all members and types
{
    IList<UpdateAudioDataMessageHandler> UpdateAudioDataMessageHandlers { get; }

    IList<UpdateVideoDataMessageHandler> UpdateVideoDataMessageHandlers { get; }

    IList<UpdateVideoSizeMessageHandler> UpdateVideoSizeMessageHandlers { get; }
}