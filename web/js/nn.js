/* Copyright 2017 Daniel J. Liebling. All rights reserved. */
"use strict";

var nnApp = angular.module('nnApp', []);

nnApp.controller('MainController', ['$scope', '$http', 'comments', function ($scope, $http, comments) {
    $scope.submit = function () {
        comments.postRebuttal();
    };

    $scope.nextComment = function () {
        comments.getComment();
    };

    comments.getComment().then(function (data) {
        $scope.comment = data;
    });
}]);


nnApp.factory('comments', ['$http', '$q', function ($http, $q) {
    var getComment = function () {
        var projectId = 3024625;
        var commentId = 675324;

        return $q(function (resolve, reject) {
            // fire off two async requests: one for the project and one for the comment text
            var projectPrm = $http.get('https://nimbyninja.blob.core.windows.net/wa-seattle/project-' + projectId + '.json');
            var commentPrm = $http.get('https://nimbyninja.blob.core.windows.net/wa-seattle/doc-' + commentId + '.txt');

            // join back async threads when they complete, then return to controller
            $q.all([projectPrm, commentPrm]).then(function (results) {
                resolve({
                    "project": results[0].data.result,
                    "id": projectId,
                    "commentText": "Comment text" //results[1].data
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