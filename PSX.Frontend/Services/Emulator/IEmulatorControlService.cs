namespace PSX.Frontend.Services.Emulator;

public interface IEmulatorControlService
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