using PSX.Core.Interfaces;

namespace PSX.Frontend.Core.Services.Emulator;

public interface IEmulatorUpdate : IHostWindow
// TODO rename all members and types
{
    IList<EmulatorUpdateAudioDataMessageHandler> UpdateAudioDataMessageHandlers { get; }

    IList<EmulatorUpdateVideoDataMessageHandler> UpdateVideoDataMessageHandlers { get; }

    IList<EmulatorUpdateVideoSizeMessageHandler> UpdateVideoSizeMessageHandlers { get; }
}