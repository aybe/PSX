using PSX.Core.Interfaces;

namespace PSX.Frontend.Services.Emulation;

public interface IEmulatorDisplayService : IHostWindow
{
    IList<UpdateAudioDataHandler> UpdateAudioDataHandlers { get; }

    IList<UpdateVideoDataHandler> UpdateVideoDataHandlers { get; }

    IList<UpdateVideoSizeHandler> UpdateVideoSizeHandlers { get; }
}