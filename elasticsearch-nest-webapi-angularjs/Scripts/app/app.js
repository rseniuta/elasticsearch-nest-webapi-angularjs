angular.module('elasticsearch', [])
    .controller('searchController', function ($scope, $http) {
        $scope.search = function () {
            $scope.isLoading = true;
            $http.get("/api/search?q=" + $scope.query).success(function (data) {
                $scope.isLoading = false;
                $scope.items = data.Results;
                if (data.Results.length === 0) {
                    $scope.items = [];
                    $scope.message = "no results";
                }
                else {
                    $scope.message = "Found " + data.Total + " records in " + data.ElapsedMilliseconds + "ms.";
                }
            });
        }
    });