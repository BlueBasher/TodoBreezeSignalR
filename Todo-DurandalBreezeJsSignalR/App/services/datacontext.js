define([
        'durandal/system',
        'services/logger',
        'config'],
    function (system, logger, config) {
        "use strict";

        var manager = configureBreezeManager(),
            isSaving = ko.observable(false),
            hasChanges = ko.observable(false),
            //#region Public Methods
            getAllTodos = function(itemsObservable, includeArchived, extendItem) {
                var query = breeze.EntityQuery
                    .from("Todos")
                    .orderBy("createdAt");

                if (!includeArchived) { // exclude archived Todos
                    // add filter clause limiting results to non-archived Todos
                    query = query.where("isArchived", "==", false);
                }

                query = query.using(manager);
                return query.execute()
                    .then(function() { // ignore remote query results
                        return query.using(breeze.FetchStrategy.FromLocalCache).execute();
                    })
                    .then(querySucceeded)
                    .fail(queryFailed);

                function querySucceeded(data) {
                    if (itemsObservable) {
                        itemsObservable([]);
                        data.results.forEach(function(item) {
                            if ($.isFunction(extendItem)) {
                                extendItem(item);
                            }
                            itemsObservable.push(item);
                        });
                    }

                    manager.entityChanged.subscribe(function(eventArgs) {
                        if (eventArgs.entity.entityType.defaultResourceName === 'Todos') {

                            // Deleted entities
                            if ((eventArgs.entityAction === breeze.EntityAction.Detach &&
                                 itemsObservable.indexOf(eventArgs.entity) >= 0) ||
                                (eventArgs.entityAction === breeze.EntityAction.EntityStateChange &&
                                 eventArgs.entity.entityAspect.entityState.isDeleted() &&
                                 itemsObservable.indexOf(eventArgs.entity) >= 0)) {
                                itemsObservable.remove(eventArgs.entity);
                            }

                            // Added entities
                            if ((eventArgs.entityAction === breeze.EntityAction.AttachOnQuery &&
                                 itemsObservable.indexOf(eventArgs.entity) == -1) ||
                                (eventArgs.entityAction === breeze.EntityAction.EntityStateChange &&
                                 eventArgs.entity.entityAspect.entityState.isAdded() &&
                                 itemsObservable.indexOf(eventArgs.entity) == -1)) {
                                if ($.isFunction(extendItem)) {
                                    extendItem(eventArgs.entity);
                                }
                                itemsObservable.push(eventArgs.entity);
                            }
                        }
                    });

                    log('[Todos] refreshed', data, true);
                }
            },
            createTodo = function(initialValues) {
                return manager.createEntity('TodoItem', initialValues);
            },
            saveChanges = function() {
                if (manager.hasChanges()) {
                    if (isSaving()) {
                        setTimeout(saveChanges, 50);
                        return;
                    }
                    isSaving(true);
                    manager.saveChanges()
                        .then(saveSucceeded)
                        .fail(saveFailed)
                        .fin(saveFinished);
                }
                ;

                function saveSucceeded(saveResult) {
                    log("# of Todos saved = " + saveResult.entities.length, saveResult);
                }

                function saveFailed(error) {
                    var reason = error.message;
                    var detail = error.detail;

                    if (reason === "Validation error") {
                        handleSaveValidationError(error);
                        return;
                    }
                    if (detail && detail.ExceptionType &&
                        detail.ExceptionType.indexOf('OptimisticConcurrencyException') !== -1) {
                        // Concurrency error 
                        reason =
                            "Another user, perhaps the server, may have deleted one or all of the todos.";
                        manager.rejectChanges(); // DEMO ONLY: discard all pending changes
                    }

                    logError("Failed to save changes. " + reason + " You may have to restart the app.", error);
                }

                function saveFinished() {
                    isSaving(false);
                }

                function handleSaveValidationError(error) {
                    var message = "Not saved due to validation error";
                    try { // fish out the first error
                        var firstErr = error.entitiesWithErrors[0].entityAspect.getValidationErrors()[0];
                        message += ": " + firstErr.errorMessage;
                    } catch(e) { /* eat it for now */
                    }
                    logError(message);
                }
            },
            refreshEntity = function(entityName, id, state) {

                function fetchSucceeded(data) {
                    log('[' + entityName + '] refreshed', data.entity);
                }

                var entity = manager.getEntityByKey(entityName, id);

                // Only refresh the entity when it's a newly added entity
                // or it's modified and the client has the specified entity already in local cache
                if ((state === 'Added') ||
                    (entity && state === 'Modified')) {
                    // Call fetchEntityByKey with false so we always request it from the server
                    manager.fetchEntityByKey(entityName, id, false)
                        .then(fetchSucceeded)
                        .fail(queryFailed);
                }

                if (entity && state === 'Deleted') {
                    // If the entity already has state deleted we don't need to do anything\
                    if (entity.entityAspect.entityState !== breeze.EntityState.Deleted) {
                        entity.entityAspect.entityState = breeze.EntityState.Added;
                        entity.entityAspect.setDeleted();

                        log('[' + entityName + '] removed');
                    }
                }
            };
            
        //#endregion 

        manager.hasChangesChanged.subscribe(function (eventArgs) {
            hasChanges(eventArgs.hasChanges);
        });

        var datacontext = {
            isSaving: isSaving,
            hasChanges: hasChanges,
            getAllTodos: getAllTodos,
            createTodo: createTodo,
            saveChanges: saveChanges,
            refreshEntity: refreshEntity
        };

        return datacontext;

        //#region Internal Methods
        function getQueryLocal(query) {
            return manager.executeQueryLocally(query);
        }

        function queryFailed(error) {
            var msg = 'Error retreiving data. ' + error.message;
            logError(msg, error);
        }
        
        function configureBreezeManager() {
            breeze.NamingConvention.camelCase.setAsDefault();
            var mgr = new breeze.EntityManager(config.serviceName);

            return mgr;
        }

        function log(msg, data) {
            logger.log(msg, data, system.getModuleId(datacontext), true);
        }

        function logError(msg, error) {
            logger.logError(msg, error, system.getModuleId(datacontext), true);
        }
        //#endregion
    });