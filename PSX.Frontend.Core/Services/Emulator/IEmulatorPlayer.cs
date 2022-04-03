namespace PSX.Frontend.Core.Services.Emulator;

public interface IEmulatorPlayer // TODO as a service?
{
    bool CanStart { get; }

    bool CanStop { get; }

    bool CanPause { get; }

    bool CanContinue { get; }

    bool CanFrame { get; }

    void Setup(IEmulatorService service, string content);

    void Start();

    void Stop();

    void Pause();

    void Continue();

    void Frame();
}