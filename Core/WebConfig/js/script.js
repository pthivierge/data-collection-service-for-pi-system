(function () {
    'use strict';

    var app=angular.module('app', [
        // Angular modules 
        'ngRoute'

        // Custom modules 

        // 3rd Party Modules
        
    ]);


    app.config(function ($routeProvider) {
        $routeProvider

            // route for the home page
            .when('/', {
                templateUrl: 'pages/home.html',
                controller: 'mainController'
            })

            // route for the about page
            .when('/about', {
                templateUrl: 'pages/about.html',
                controller: 'aboutController'
            });


    });


    app.controller('aboutController', function ($scope) {
        $scope.message = 'Look! I am an about page.';
    });

    app.controller('mainController', function ($scope) {
        $scope.message = 'Contact us! JK. This is just a demo.';
    });


})();