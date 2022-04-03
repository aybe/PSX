namespace PSX.Frontend.Core.Services.Emulator;

public interface IEmulatorServiceController
{
    bool CanStart { get; }

    bool CanStop { get; }

    bool CanPause { get; }

    bool CanContinue { get; }

    bool CanFrame { get; }

    void Setup(string content);

    void Start();

    void Stop();

    void Pause();

    void Continue();

    void Frame();
}