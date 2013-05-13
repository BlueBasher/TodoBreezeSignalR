define(['durandal/system',
        'services/logger',
        'services/datacontext'],
    function (system, logger, datacontext) {
        "use strict";

        var suspendItemSave,
            items = ko.observableArray([]),
            newTodo = ko.observable(''),
            includeArchived = ko.observable(false),
            markAllCompleted = ko.observable(false),
            archiveCompletedMessage = ko.computed(function () {
                var count = getStateOfItems().itemsDoneCount;
                if (count > 0) {
                    return "Archive " + count + " completed item" + (count > 1 ? "s" : "");
                }
                return null;
            }),
            itemsLeftMessage = ko.computed(function () {
                var count = getStateOfItems().itemsLeftCount;
                if (count > 0) {
                    return count + " item" + (count > 1 ? "s" : "") + " left";
                }
                return null;
            }),

            //#region Public Methods
            activate = function () {
                return refreshItems();
            },
            addItem = function () {
                var item = datacontext.createTodo({
                    description: vm.newTodo(),
                    createdAt: new Date()
                });

                if (item.entityAspect.validateEntity()) {
                    datacontext.saveChanges();
                    newTodo('');
                } else {
                    handleItemErrors(item);
                }
            },
            edit = function (item) {
                if (item) {
                    item.isEditing(true);
                }
            },
            removeItem = function (item) {
                if (item) {
                    item.entityAspect.setDeleted();
                    datacontext.saveChanges();
                }
            },
            completeEdit = function (item) {
                if (item) {
                    item.isEditing(false);
                }
            },
            archiveCompletedItems = function() {
                var state = getStateOfItems();
                suspendItemSave = true;
                state.itemsDone.forEach(function (item) {
                    if (!includeArchived()) {
                        items.remove(item);
                    }
                    item.isArchived(true);
                });
                suspendItemSave = false;
                datacontext.saveChanges();
            };
        //#endregion 

        //#region subscriptions 
        includeArchived.subscribe(function () {
            refreshItems();
        }),
        markAllCompleted.subscribe(function (value) {
            suspendItemSave = true;
            items().forEach(function(item) {
                if (!item.isArchived()) {
                    item.isDone(value); // only unarchived items 
                }
            });
            suspendItemSave = false;
            datacontext.saveChanges();
        });
        //#endregion 

        var vm = {
            title: 'Todo',
            isProcessingTask: datacontext.isSaving,
            items: items,
            includeArchived: includeArchived,
            archiveCompletedMessage: archiveCompletedMessage,
            itemsLeftMessage: itemsLeftMessage,
            newTodo: newTodo,
            activate: activate,
            addItem: addItem,
            edit: edit,
            removeItem: removeItem,
            completeEdit: completeEdit,
            archiveCompletedItems: archiveCompletedItems,
            markAllCompleted: markAllCompleted
        };
        return vm;

        //#region Internal Methods

        function refreshItems() {
            return datacontext.getAllTodos(items, includeArchived(), extendItem);
        }

        function extendItem(item) {
            if (item.isEditing) {
                return; // already extended
            }

            item.isEditing = ko.observable(false);

            // listen for changes with Breeze PropertyChanged event
            item.entityAspect.propertyChanged.subscribe(function () {
                if (item.propertyChangedPending || suspendItemSave) {
                    return;
                }

                // throttle property changed response
                // allow time for other property changes (if any) to come through
                item.propertyChangedPending = true;
                setTimeout(validateAndSaveModifiedItem, 10);

                function validateAndSaveModifiedItem() {
                    if (item.entityAspect.entityState.isModified()) {
                        if (item.entityAspect.validateEntity()) {
                            datacontext.saveChanges();
                        } else { // errors
                            handleItemErrors(item);
                            item.isEditing(true); // go back to editing
                        }
                    }
                    item.propertyChangedPending = false;
                }

            });
        }

        function handleItemErrors(item) {
            if (!item) { return; }
            var errs = item.entityAspect.getValidationErrors();
            if (errs.length == 0) {
                logger.info("No errors for current item");
                return;
            }
            var firstErr = item.entityAspect.getValidationErrors()[0];
            logger.error(firstErr.errorMessage);
            item.entityAspect.rejectChanges(); // harsh for demo 
        }

        function getStateOfItems() {
            var itemsDone = [], itemsLeft = [];

            items().forEach(function (item) {
                if (item.isDone()) {
                    if (!item.isArchived()) {
                        itemsDone.push(item); // only unarchived items                
                    }
                } else {
                    itemsLeft.push(item);
                }
            });

            return {
                itemsDone: itemsDone,
                itemsDoneCount: itemsDone.length,
                itemsLeft: itemsLeft,
                itemsLeftCount: itemsLeft.length
            };
        }
        //#endregion
    }
);