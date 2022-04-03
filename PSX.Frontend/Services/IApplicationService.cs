namespace PSX.Frontend.Services;

public interface IApplicationService
{
    void Shutdown();

    bool TryGetView<TView>(out TView? result) where TView : class;
}