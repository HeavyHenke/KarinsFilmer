angular.module("MovieApp", [])
    .controller("RateCtrl", function ($scope, $http) {
        $scope.title = "loading movies...";
        $scope.movieTips = [];
        $scope.movieSearchResult = [];
        $scope.working = false;

        //$scope.searchMovies = function () {
        //    $scope.working = true;
        //    $scope.title = "loading movies...";
        //    $scope.movieSearchResult = [];

        //    $http.get("http://www.omdbapi.com/?s=unchained*&type=movie").success(function (data, status, headers, config) {
        //        Console.log(data);
        //        $scope.movieSearchResult = data.search;
        //        $scope.title = data.title;
        //        $scope.working = false;
        //    }).error(function (data, status, headers, config) {
        //        $scope.title = "Oops... something went wrong";
        //        $scope.working = false;
        //    });
        //};

        $scope.saveRating = function (movie) {
            $scope.working = true;

            $http.post("/api/MovieApi", { 'movieId': movie.imdbID, 'rating': movie.rating }).success(function (data, status, headers, config) {
                $scope.working = false;
            }).error(function (data, status, headers, config) {
                $scope.title = "Oops... something went wrong";
                $scope.working = false;
            });
        };

        $scope.getMovieTips = function () {
            $scope.working = true;
            $scope.movieTips = [];

            $http.get("/api/MovieApi")
                .success(function(data, status, headers, config) {
                    console.log(data);
                    $scope.movieTips = data;
                    $scope.working = false;
                    console.log(data);
                })
                .error(function (data, status, headers, config) {
                    $scope.title = "Oops... something went wrong";
                    $scope.working = false;
                });
        };
    });