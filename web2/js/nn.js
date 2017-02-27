/* Copyright 2017 Daniel J. Liebling. All rights reserved. */

"use strict";

var nnApp = angular.module('nnApp', ['ngStorage', 'ngRoute']);

nnApp.controller('CommentController', ['$scope', '$http', '$localStorage', '$routeParams', '$location', 'comments',
    function ($scope, $http, $localStorage, $routeParams, $location, comments) {
        $scope.storage = $localStorage;

        var map = new Microsoft.Maps.Map(document.getElementById('map'), {
            credentials: 'AulBFfFGC4C1qaIJ3_oPNgAJWyffDpSzzMnyUiM1Rus1uJOheMYtkGLmfWVjCVP2',
            navigationBarMode: Microsoft.Maps.NavigationBarMode.compact
        });

        var updateMap = function () {
            map.setView({
                center: new Microsoft.Maps.Location($scope.comment.project.coordinate.latitude, $scope.comment.project.coordinate.longitude),
                zoom: 15
            });
            if (map.entities.getLength() == 1)
            {
                map.entities.removeAt(0);
            }
            map.entities.push(new Microsoft.Maps.Pushpin(map.getCenter(), null));
        };


        $scope.submit = function () {
            comments.postRebuttal($scope.comment.id, $scope.comment.commentId, $scope.emailAddress, $scope.rebuttal).then(function () {
                $scope.storage.rebutted.push($scope.comment.commentId);
                $scope.nextComment();
            });
        };

        comments.getSummary().then(function (data) {
            $scope.summary = data;
            comments.getComment($routeParams.projectId, $routeParams.commentId).then(function (data) {
                $scope.comment = data;
                updateMap();
            });
        });
    }
]);


// picks a random project and redirects route
nnApp.controller('MainController', ['$location', 'comments', function ($location, comments) {
    comments.getSummary().then(function (data) {
        var randProj = comments.pickRandom();
        $location.url('/project/' + randProj.projectId + '/comment/' + randProj.commentId);
    });
}]);


nnApp.factory('comments', ['$http', '$q', '$localStorage', function ($http, $q, $localStorage) {
    var dataRoot = 'https://nimbyninja.blob.core.windows.net/';

    var summary = null; // cache summary data

    // init array of rebutted comments
    if ($localStorage.rebutted === undefined) {
        $localStorage.rebutted = [];
    }

    var removeRebutted = function (data) {
        for (var i = 0; i < data.projects.length; i++) {
            var project = data.projects[i];
            for (var j = 0; j < project.comments.length; j++) {
                // remove any comments on which the user has already commented
                project.comments = project.comments.filter(function (c) {
                    return ($localStorage.rebutted.indexOf(c) == -1);
                });
            }
        }
    };


    var getSummary = function () {
        return $q(function (resolve, reject) {
            if (summary !== null) {
                resolve(summary);
            }
            else {
                $http.get(dataRoot + 'wa-seattle/summary.json')
                .then(function (resp) {
                    removeRebutted(resp.data);
                    summary = resp.data;
                    resolve(summary);
                }, reject);
            }
        });
    };


    var pickRandom = function () {
        // pick a project
        var pidx = Math.floor(Math.random() * summary.projects.length);
        var project = summary.projects[pidx];
        // pick a comment
        var cidx = Math.floor(Math.random() * project.comments.length);
        var commentId = project.comments[cidx];

        return {'projectId': project.id, 'commentId': commentId};
    };


    var getComment = function (projectId, commentId) {
        return $q(function (resolve, reject) {
            // fire off two async requests: one for the project and one for the comment text
            var projectPrm = $http.get(dataRoot + 'wa-seattle/project-' + projectId + '.json');
            var commentPrm = $http.get(dataRoot + 'wa-seattle/doc-' + commentId + '.txt');

            // join back async threads when they complete, then return to controller
            $q.all([projectPrm, commentPrm]).then(function (results) {
                resolve({
                    "project": results[0].data.result,
                    "id": projectId,
                    "commentId": commentId,
                    "commentText": results[1].data
                });
            }, reject);
        })
    };

    var postRebuttal = function (projectId, commentId, onBehalfOf, rebuttal) {
        $http.post("api/Rebuttal",
            {
                "projectId": projectId,
                "commentId": commentId,
                "emailAddress": onBehalfOf,
                "rebuttal": rebuttal
            })
        .then();
    };

    return {
        "getSummary": getSummary,
        "getComment": getComment,
        "pickRandom": pickRandom,
        "postRebuttal": postRebuttal 
    };
}]);


nnApp.config(function ($routeProvider, $locationProvider) {
    $routeProvider.when('/project/:projectId/comment/:commentId', {
        controller: 'CommentController',
        templateUrl: 'mainTemplate.html'
    })
    .when('/', {
        controller: 'MainController',
        template: '<div>picking a battle...</div>'
    });
});