/* Copyright 2017 Daniel J. Liebling. All rights reserved. */


var nnApp = angular.module('nnApp', []);

nnApp.controller('MainController', ['$scope', '$http', 'comments', function ($scope, $http, comments) {
    $scope.submit = function () {
        comments.postRebuttal();
    };

    $scope.nextComment = function () {
        comments.getComment();
    };

    /*$scope.comment = {
        id: 123456,
        projectId: 54321,
        text: "Loading comment..."
    };*/

    comments.getComment().then(function (data) {
        $scope.comment = data;
    });
}]);


nnApp.factory('comments', ['$http', '$q', function ($http, $q) {
    var getComment = function () {
        var projectId = 3024625;

        return $q(function (resolve, reject) {
            $http.get('wa-seattle/project-' + projectId + '.json')
            //$http.get('https://data.nimby.ninja/wa-seattle/project-' + projectId + '.json')
                .then(function (resp) {
                    resolve({
                        "project": resp.data.result,
                        "id": 1247532,
                        "text": "Lorem ipsum i need moar parking directly in front of my house"
                    });
                }, reject);
        })
    };

    var postRebuttal = function (inResponseTo, onBehalfOf, rebuttal) {

    };

    return {
        "getComment": getComment,
        "postRebuttal": postRebuttal 
    };
}]);