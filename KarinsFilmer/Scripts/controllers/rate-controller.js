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

(function () {
    "use strict";

    function RatingController() {
        this.rating1 = 5;
        this.rating2 = 2;
        this.isReadonly = true;
        this.rateFunction = function (rating) {
            console.log("Rating selected: " + rating);
        };
    }

    function starRating() {
        return {
            restrict: "EA",
            template:
              "<ul class='star-rating' ng-class='{readonly: readonly}'>" +
              "  <li ng-repeat='star in stars' class='star' ng-class='{filled: star.filled}' ng-click='toggle($index)'>" +
              "    <i class='fa fa-star'></i>" + // or &#9733
              "  </li>" +
              "</ul>",
            scope: {
                ratingValue: "=ngModel",
                max: "=?", // optional (default is 5)
                onRatingSelect: "&?",
                readonly: "=?"
            },
            link: function (scope, element, attributes) {
                if (scope.max == undefined) {
                    scope.max = 5;
                }
                function updateStars() {
                    scope.stars = [];
                    for (var i = 0; i < scope.max; i++) {
                        scope.stars.push({
                            filled: i < scope.ratingValue
                        });
                    }
                };
                scope.toggle = function (index) {
                    if (scope.readonly == undefined || scope.readonly === false) {
                        scope.ratingValue = index + 1;
                        scope.onRatingSelect({
                            rating: index + 1
                        });
                    }
                };
                scope.$watch("ratingValue", function (oldValue, newValue) {
                    if (newValue) {
                        updateStars();
                    }
                });
            }
        };
    }

    angular
        .module("app", [])
        .controller("RatingController", RatingController)
        .directive("starRating", starRating);
})();
