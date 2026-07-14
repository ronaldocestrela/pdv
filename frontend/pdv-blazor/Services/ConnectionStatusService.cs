using System;
using System.Threading.Tasks;

namespace Pdv.Web.Services;

public class ConnectionStatusService
{
    public virtual bool IsOnline => true;
    public virtual int PendingItemsCount => 0;
    
    public virtual event Action? OnStatusChanged;

    protected void NotifyStatusChanged()
    {
        OnStatusChanged?.Invoke();
    }

    public virtual Task SyncAsync()
    {
        return Task.CompletedTask;
    }
}
