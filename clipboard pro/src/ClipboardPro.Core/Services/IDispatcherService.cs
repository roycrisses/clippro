namespace ClipboardPro.Core.Services;

public interface IDispatcherService
{
    void Invoke(Action action);
    Task InvokeAsync(Action action);
}
