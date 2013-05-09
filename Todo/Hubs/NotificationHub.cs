namespace Todo.Hubs
{
    using Microsoft.AspNet.SignalR;

    public class NotificationHub : Hub
    {
        public void RefreshEntity(string entityName, object id)
        {
            Clients.All.refreshEntity(entityName, id);
        }
    }
}