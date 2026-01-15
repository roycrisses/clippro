using Avalonia.Threading;
using ClipboardPro.Core.Services;

namespace ClipboardPro.Avalonia.Services;

public class AvaloniaDispatcherService : IDispatcherService
{
    public void Invoke(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Invoke(action);
        }
    }

    public async Task InvokeAsync(Action action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }
}
