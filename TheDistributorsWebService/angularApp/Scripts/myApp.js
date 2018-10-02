var myApp = angular.module('myApp', ['ngRoute']);

//Config Routing 
myapp.config(['$routeProvider', function ($routeProvider)
{
    $routeProvider
        .when('/', {
            redirectTo: '/home'
        })
        .when('/home',{
            templateUrl: '/template/home.html',
            controller: 'homeController'
        })
        .when('/authenticated', {
            templateUrl: '/template/authenticated.html',
            controller: 'authenticatedController'
        })
        .when('/authorized', {
            templateUrl: '/template/authorize.html',
            controller: 'authorizeController'
        })
        .when('/login', {
            templateUrl: '/template/login.html',
            controller: 'loginController'
        })
        .when('/unauthorized', {
            templateUrl: '/template/unauthorize.html',
            controller: 'unauthorizeController'
        })
}])

// Gloal variable for string service base Path
myApp.constant('serviceBasePath','http://localhost:52104');
//Add Controllers
myApp.controller('homeController', ['$scope','dataService', function ($scope, dataService) {
    $scope.data = "";
    dataService.GetAuthenticateData().then(function (data) {
        $scope.data = data;
    })

}])

myApp.controller('authenticateController', ['$scope','dataService', function ($scope, dataService) {
    $scope.data = "";
    dataService.GetAuthenticateData().then(function (data) {
        $scope.data = data;
    })

}])

myApp.controller('authorizeController', ['$scope','dataService', function ($scope, dataService) {
    $scope.data = "";
    dataService.GetAuthorizedData().then(function (data) {
        $scope.data = data;
    })

}])

myApp.controller('loginController', ['$scope', function ($scope) {


}])

myApp.controller('UnauthorizeController', ['$scope', function ($scope) {


}])
//Add Service
myApp.factory('dataService', ['$http', 'serviceBasePath', function ($http, serviceBasePath) {
    var fac = {};
    fac.GetAnnonymous = function() {
        return $http.get(serviceBasePath + '/api/data/forall').then(function (response) {
            return response.data;
        })
    }

    fac.GetAuthenticateData = function () {
        return $http.get(serviceBasePath + '/api/data/authenticate').then(function (response) {
            return response.data;
        })
    }

    fac.GetAuthorizedData = function () {
        return $http.get(serviceBasePath + '/api/data/authorize').then(function (response) {
            return response.data;
        })
    }

   
    return fac;
}])
//Add HttpInterceptors
