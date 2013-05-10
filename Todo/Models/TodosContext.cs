namespace Todo.Models
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Runtime.Remoting.Contexts;
    using Todo.Hubs;

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
            var statesToNotify = new List<EntityState>
                {
                    EntityState.Added, 
                    EntityState.Modified,
                    EntityState.Deleted
                };
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            // Get the entities that have changed
            var changeSet = this.ChangeTracker.Entries()
                                .Where(e => statesToNotify.Contains(e.State))
                                .Select(e => new
                                    {
                                        e.Entity,
                                        e.State,
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

            // Save the entities
            var result = base.SaveChanges();

            // Notify Clients of the changes
            using (var notificationHubClient = new NotificationHubClient())
            {
                notificationHubClient.Initialize();

                foreach (var entity in changeSet)
                {
                    var entityName = entity.Entity.GetType().Name;

                    if (entity.State == EntityState.Deleted)
                    {
                        notificationHubClient.NotifyRemoveEntity(entityName, entity.Keys);
                    }
                    else
                    {
                        // Get the keys again so we get the actual keys. For added entities EF generates temporary keys...
                        var keys = objectContext.ObjectStateManager.GetObjectStateEntry(entity.Entity)
                                                       .EntityKey.EntityKeyValues
                                                       .Select(key => key.Value)
                                                       .ToList();

                        notificationHubClient.NotifyRefreshEntity(entityName, keys, entity.State.ToString());
                    }
                }
            }

            return result;
        }
    }
}