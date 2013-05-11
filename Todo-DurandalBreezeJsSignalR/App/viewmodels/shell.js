define([
        'durandal/system',
        'durandal/plugins/router',
        'services/logger',
        'services/datacontext',
    	'services/notificationHub',
        'config'],
    function (system, router, logger, datacontext, notificationHub, config) {
        "use strict";

        var isSearching = ko.observable(false),
            canSearch = ko.computed(function () {
                return router.activeItem && router.activeItem() && $.isFunction(router.activeItem().search);
            }),
            isProcessingTask = ko.computed(function () {
                return router.activeItem && router.activeItem() &&
                    $.isFunction(router.activeItem().isProcessingTask) &&
                    router.activeItem().isProcessingTask();
            }),
            spinnerActive = ko.computed(function () {
                return router.isNavigating() || isSearching() || isProcessingTask();
            }),

        //#region Public Methods
        activate = function () {
            return boot()
                .fail(failedInitialization);
        },
        search = function () {
            if (this.canSearch()) {
                var value = $('.navbar-search > .search-query').val();
                isSearching(true);
                router.activeItem().search(value)
                    .done(function () { isSearching(false); });
            }
            else {
                app.showMessage('Search not available');
            }
        };
        //#region Methods

        var shell = {
            router: router,
            canSearch: canSearch,
            spinnerActive: spinnerActive,
            activate: activate,
            search: search
        };
        return shell;

        function boot() {
            router.mapNav('todo');

            logger.log(config.appTitle + ' started!', null, system.getModuleId(shell));

            return router.activate(config.startModule)
                .then(function() { notificationHub.init(); });
        }

        function failedInitialization(error) {
            var msg = 'Error starting : ' + error.message;
            logger.error(msg);
        }
    }
);