namespace Todo_DurandalBreezeJsSignalR.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.AspNet.SignalR.Client.Hubs;

    /// <summary>
    /// The SignalR client for the NotificationHub
    /// </summary>
    public class NotificationHubClient : IDisposable
    {
        private HubConnection _hubConnection;
        private IHubProxy _notificationHub;

        #region Dispose
        /// <summary>
        /// Finalizes an instance of the <see cref="NotificationHubClient"/> class.
        /// </summary>
        ~NotificationHubClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_hubConnection == null)
            {
                return;
            }

            _hubConnection.Disconnect();
            _hubConnection = null;
        }
        #endregion
        /// <summary>
        /// Initialize the client and connect to hub.
        /// </summary>
        public void Initialize(string url)
        {
            _hubConnection = new HubConnection(url);

            // Create a proxy to the NotificationHub
            _notificationHub = _hubConnection.CreateHubProxy("NotificationHub");

            // if this client also needs to do something with refreshing entities
            _notificationHub.On<string, object, string>(
                "refreshEntity",
                (entityName, id, state) => Debug.Print("Received: RefreshEntity {0}, {1}, {2}", entityName, id, state));

            // Start the connection
            _hubConnection.Start().Wait();
        }

        /// <summary>
        /// Notify all clients to Refresh Entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <param name="state"></param>
        public void NotifyRefreshEntity(string entityName, object id, string state)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentNullException("entityName");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("state");
            }
            if (_notificationHub == null)
            {
                throw new InvalidOperationException("Initialize NotificationHub prior to calling NotifyRefreshEntity.");
            }

            _notificationHub.Invoke("refreshEntity",
                                    new NotificationHub.EntityState
                                        {
                                            Name = entityName,
                                            Id = id,
                                            State = state
                                        });
        }

        public void NotifyRefreshEntities(List<NotificationHub.EntityState> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            _notificationHub.Invoke("refreshEntities", entities);
        }

    }
}
