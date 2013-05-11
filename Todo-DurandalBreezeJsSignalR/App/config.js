define(function () {
    "use strict";

    toastr.options.timeOut = 4000;
    toastr.options.positionClass = 'toast-bottom-right';
    toastr.options.backgroundpositionClass = 'toast-bottom-right';

    var appTitle = 'Todo',
        serviceName = 'breeze/todos/',
        startModule = 'todo';

    return {
        appTitle: appTitle,
        serviceName: serviceName,
        startModule: startModule
    };

});