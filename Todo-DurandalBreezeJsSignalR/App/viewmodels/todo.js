define(['durandal/system',
        'services/logger',
        'services/datacontext',
        'config'],
    function (system, logger, datacontext, config) {
        "use strict";

        var items = ko.observableArray([]),
            newTodo = ko.observable(''),

            //#region Public Methods
            activate = function () {
                return datacontext.getAllTodos(items, false, extendItem);
            },
            addItem = function() {
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
            };
        //#endregion 

        var vm = {
            title: 'Todo',
            isProcessingTask: datacontext.isSaving,
            items: items,
            newTodo: newTodo,
            activate: activate,
            addItem: addItem,
            edit: edit,
            removeItem: removeItem,
            completeEdit: completeEdit
        };
        return vm;
        
        //#region Internal Methods
        function extendItem(item) {
            if (item.isEditing) {
                return; // already extended
            }

            item.isEditing = ko.observable(false);
            
            // listen for changes with Breeze PropertyChanged event
            item.entityAspect.propertyChanged.subscribe(function () {
                if (item.propertyChangedPending) {
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
        //#endregion
    }
);