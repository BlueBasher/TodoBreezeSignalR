define([
        'durandal/system',
        'services/logger',
        'services/datacontext',
        'config'],
    function (system, logger, datacontext, config) {
        "use strict";

        var

        //#region Public Methods
        init = function () {
            var hub = $.connection.notificationHub;

            hub.client.refreshEntity = function (entityName, id, state) {
                datacontext.refreshEntity(entityName, id, state);
            };

            // Start the connection
            $.connection.hub.start()
                .done(function() {
                    logger.log('NotificationHub started!', null, system.getModuleId(notificationHub), true);
                });

        };
        //#region Methods

        var notificationHub = {
            init: init
        };
        return notificationHub;
    });