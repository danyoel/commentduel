/* Copyright 2017 Daniel J. Liebling. All rights reserved. */

"use strict";

var nnApp = angular.module('nnApp', ['ngStorage']);

nnApp.controller('MainController', ['$scope', '$http', '$localStorage', 'comments',
    function ($scope, $http, $localStorage, comments) {
        $scope.storage = $localStorage;
        // init array of rebutted comments
        if ($scope.storage.rebutted === undefined)
        {
            $scope.storage.rebutted = [];
        }


        $scope.nextComment = function () {
            // pick a project
            var pidx = Math.floor(Math.random() * $scope.summary.projects.length);
            var project = $scope.summary.projects[pidx];
            // pick a comment
            var cidx = Math.floor(Math.random() * project.comments.length);
            var commentId = project.comments[cidx];

            // kick off async request for that comment
            comments.getComment(project.id, commentId).then(function (data) {
                $scope.comment = data;
            });
        };


        $scope.submit = function () {
            comments.postRebuttal().then(function () {
                $scope.storage.rebutted.push($scope.comment.commentId);
                $scope.nextComment();
            });
        };


        // kick off data load request, then populate comments when ready.
        comments.getSummary().then(function (data) {
            // remove rebutted comments
            for (var i = 0; i < data.projects.length; i++) {
                var project = data.projects[i];
                for (var j = 0; j < project.comments.length; j++) {
                    // remove any comments on which the user has already commented
                    project.comments = project.comments.filter(function (c) {
                        return $scope.storage.rebutted.indexOf(c) == -1;
                    });
                }
            }

            $scope.summary = data;
            $scope.nextComment();
        });
    }
]);


nnApp.factory('comments', ['$http', '$q', function ($http, $q, $localStorage) {
    var dataRoot = 'https://nimbyninja.blob.core.windows.net/';

    var getSummary = function () {
        return $q(function (resolve, reject) {
            $http.get(dataRoot + 'wa-seattle/summary.json')
            .then(function (resp) {
                resolve(resp.data);
            }, reject);
        });
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

    var postRebuttal = function (inResponseTo, onBehalfOf, rebuttal) {

    };

    return {
        "getSummary": getSummary,
        "getComment": getComment,
        "postRebuttal": postRebuttal 
    };
}]);