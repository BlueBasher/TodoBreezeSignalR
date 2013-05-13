namespace Todo_DurandalBreezeJsSignalR.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Todo_DurandalBreezeJsSignalR.Hubs;

    public class TodosContext : DbContext
    {
        // DEVELOPMENT ONLY: initialize the database
        static TodosContext()
        {
            Database.SetInitializer(new TodoDatabaseInitializer());
        }

        public DbSet<TodoItem> Todos { get; set; }

        public override int SaveChanges()
        {
            var changeSet = GetChangeSet();

            // Save the changes
            var result = base.SaveChanges();

            UpdateChangeSetWithKeys(changeSet);

            NotifyRefreshEntities(changeSet);

            return result;
        }

        #region Private Methods

        private List<ChangedEntity> GetChangeSet()
        {
            var statesToNotify = new List<EntityState>
                {
                    EntityState.Added,
                    EntityState.Modified,
                    EntityState.Deleted
                };
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            // Get the entities that have changed
            var changeSet = ChangeTracker.Entries()
                                         .Where(e => statesToNotify.Contains(e.State))
                                         .Select(e => new ChangedEntity
                                             {
                                                 Name = e.Entity.GetType().Name,
                                                 Entity = e.Entity,
                                                 State = e.State,
                                                 Keys = new List<object>()
                                             })
                                         .ToList();

            // Get the keys for deleted entities prior to actually deleting them
            foreach (var deletedEntity in changeSet.Where(e => e.State == EntityState.Deleted))
            {
                deletedEntity.Keys.AddRange(
                    objectContext.ObjectStateManager.GetObjectStateEntry(deletedEntity.Entity)
                                 .EntityKey.EntityKeyValues
                                 .Select(key => key.Value)
                                 .ToList());
            }

            return changeSet;
        }

        private void UpdateChangeSetWithKeys(IEnumerable<ChangedEntity> changeSet)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;

            // Get the keys for modified and added entities
            foreach (var entity in changeSet.Where(e => e.State != EntityState.Deleted))
            {
                entity.Keys.AddRange(
                    objectContext.ObjectStateManager.GetObjectStateEntry(entity.Entity)
                                 .EntityKey.EntityKeyValues
                                 .Select(key => key.Value)
                                 .ToList());
            }
        }

        private static void NotifyRefreshEntities(IEnumerable<ChangedEntity> changeSet)
        {
            // Notify clients about the changes
            try
            {
                using (var notificationHubClient = new NotificationHubClient())
                {
                    // TODO: Hardcoding this Url can be improved...duh..
                    notificationHubClient.Initialize("http://localhost:51577/");

                    notificationHubClient.NotifyRefreshEntities(changeSet.Select(c => new
                        NotificationHub.EntityState
                        {
                            Name = c.Name,
                            Id = c.Keys,
                            State = c.State.ToString()
                        }).ToList());
                }
            }
            catch (Exception)
            {
                //TODO: We need to log something here
            }
        }

        #endregion

        #region Private classes

        private class ChangedEntity
        {
            public string Name { get; set; }
            public object Entity { get; set; }
            public EntityState State { get; set; }
            public List<object> Keys { get; set; }
        }

        #endregion
    }
}