namespace Todo_DurandalBreezeJsSignalR.Hubs
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.AspNet.SignalR;

    /// <summary>
    /// The central SignalR hub for notifying clients
    /// </summary>
    public class NotificationHub : Hub
    {
        public void RefreshEntity(EntityState entityState)
        {
            Clients.All.refreshEntity(entityState.Name, entityState.Id, entityState.State);
            Debug.Print("RefreshEntity {0}, {1}, {2}", entityState.Name, entityState.Id, entityState.State);
        }

        public void RefreshEntities(List<EntityState> entities)
        {
            Clients.All.refreshEntities(entities);
            Debug.Print("RefreshEntities");
        }
        
        public class EntityState
        {
            public string Name { get; set; }
            public object Id { get; set; }
            public string State { get; set; }
        }
    }
}