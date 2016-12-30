/* Copyright 2017 Daniel J. Liebling. All rights reserved. */


var nnApp = angular.module('nnApp', []);

nnApp.controller('MainController', ['$scope', 'comments', function ($scope, comments) {
    $scope.submit = function () {
        comments.postRebuttal();
    };

    $scope.nextComment = function () {
        comments.getComment();
    };

    $scope.comment = {
        id: 123456,
        projectId: 54321,
        text: "Loading comment..."
    };
}]);


nnApp.factory('comments', function () {

    var getComment = function () {
        return {
            "id": 1247532,
            "text": "Lorem ipsum i need moar parking directly in front of my house"
        };
    };

    var postRebuttal = function (inResponseTo, onBehalfOf, rebuttal) {

    };

    return {
        "getComment": getComment,
        "postRebuttal": postRebuttal 
    };
});