namespace Todo.Models {
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
                    EntityState.Modified
                };
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            // Get the entities that have changed
            var changeSet = this.ChangeTracker.Entries()
                                   .Where(e => statesToNotify.Contains(e.State))
                                   .Select(e => e.Entity)
                                   .ToList();

            // Save the entities
            var result = base.SaveChanges();

            // Notify Clients of the changes
            using (var notificationHubClient = new NotificationHubClient())
            {
                notificationHubClient.Initialize();

                foreach (var entity in changeSet)
                {
                    notificationHubClient.NotifyRefreshEntity(
                        entity.GetType().Name,
                        objectContext.ObjectStateManager.GetObjectStateEntry(entity)
                                                             .EntityKey.EntityKeyValues
                                                             .Select(key => key.Value)
                                                             .ToList());
                }
            }

            return result;
        }
    }
}