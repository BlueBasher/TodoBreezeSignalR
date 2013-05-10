namespace Todo.Hubs
{
    using Microsoft.AspNet.SignalR;

    public class NotificationHub : Hub
    {
        public void RefreshEntity(string entityName, object id, string state)
        {
            Clients.All.refreshEntity(entityName, id, state);
        }

        public void RemoveEntity(string entityName, object id)
        {
            Clients.All.removeEntity(entityName, id);
        }
    }
}