﻿app.notificationHub = (function (logger, dataservice) {

    var notificationHub = {
    };

    initHub();
    
    return notificationHub; // done with setup; return module variable

    //#region private functions
    function initHub() {
        var hub = $.connection.notificationHub;

        hub.client.refreshEntity = function (entityName, id) {
            function fetchSucceeded() {
                logger.info('[' + entityName + '] refreshed');
            }

            var entity = dataservice.manager.getEntityByKey(entityName, id);

            // Only refresh the entity when the client has the specified entity already in local cache
            if (entity) {
                // Call fetchEntityByKey with false so we always request it from the server
                dataservice.manager
                    .fetchEntityByKey(entityName, id, false)
                    .then(fetchSucceeded);
            }
        };

        // Start the connection
        $.connection.hub.start().done(function () {
            logger.info('NotificationHub started!');
        });
    }
    //#endregion    

})(app.logger, app.dataservice);