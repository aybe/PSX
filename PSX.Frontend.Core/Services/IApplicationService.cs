namespace PSX.Frontend.Core.Services;

public interface IApplicationService
{
    bool TryGetView<TView>(out TView? result) where TView : class;
}