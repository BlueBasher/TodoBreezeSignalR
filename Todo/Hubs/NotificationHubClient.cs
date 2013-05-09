namespace Todo.Hubs
{
    using System;
    using Microsoft.AspNet.SignalR.Client.Hubs;

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
        public void Initialize()
        {
            _hubConnection = new HubConnection("http://localhost:63030/");

            // Create a proxy to the NotificationHub
            _notificationHub = _hubConnection.CreateHubProxy("NotificationHub");

            _notificationHub.On<string, object>("refreshEntity", (entityName, id) =>
                {
                    // If we want this client to do something with the refresh as well.....not yet necessary                    
                });
            _notificationHub.On<string, object>("removeEntity", (entityName, id) =>
            {
                // If we want this client to do something with the remove as well.....not yet necessary                    
            });

            // Start the connection
            _hubConnection.Start().Wait();
        }

        /// <summary>
        /// Notify all clients to Refresh Entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        public void NotifyRefreshEntity(string entityName, object id)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentNullException("entityName");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (_notificationHub == null)
            {
                throw new InvalidOperationException("Initialize NotificationHub prior to calling NotifyRefreshEntity.");
            }

            _notificationHub.Invoke("refreshEntity", entityName, id);
        }

        /// <summary>
        /// Notify all clients to remove Entity
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        public void NotifyRemoveEntity(string entityName, object id)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentNullException("entityName");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (_notificationHub == null)
            {
                throw new InvalidOperationException("Initialize NotificationHub prior to calling NotifyRemoveEntity.");
            }

            _notificationHub.Invoke("removeEntity", entityName, id);
        }
    }
}