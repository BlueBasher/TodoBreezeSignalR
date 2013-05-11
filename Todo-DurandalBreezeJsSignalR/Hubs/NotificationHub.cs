namespace Todo_DurandalBreezeJsSignalR.Hubs
{
    using Microsoft.AspNet.SignalR;

    /// <summary>
    /// The central SignalR hub for notifying clients
    /// </summary>
    public class NotificationHub : Hub
    {
        public void RefreshEntity(string entityName, object id, string state)
        {
            Clients.All.refreshEntity(entityName, id, state);
        }
    }
}