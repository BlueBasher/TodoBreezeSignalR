requirejs.config({
	//baseUrl: "../App",
    paths: {
        'text': 'durandal/amd/text'
    }
});

define(function (require) {
    "use strict";
    
	var app = require('durandal/app'),
		viewLocator = require('durandal/viewLocator'),
		system = require('durandal/system'),
		router = require('durandal/plugins/router'),
		logger = require('services/logger'),
	    config = require('config');

    system.debug(true);

	app.start().then(function () {

		app.title = config.appTitle;

		router.handleInvalidRoute = function (route, params) {
			logger.logError('No Route Found ' + route);
		};
	    
		viewLocator.useConvention();
		router.useConvention();
		app.adaptToDevice();

		app.setRoot('viewmodels/shell', 'entrance');
	});
});