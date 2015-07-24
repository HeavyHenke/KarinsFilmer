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

        //$scope.saveRating = function (movie) {
        //    $scope.working = true;

        //    $http.post("/api/save", { 'movieId': movie.questionId, 'rating': movie.rating }).success(function (data, status, headers, config) {
        //        $scope.working = false;
        //    }).error(function (data, status, headers, config) {
        //        $scope.title = "Oops... something went wrong";
        //        $scope.working = false;
        //    });
        //};

        $scope.getMovieTips = function() {
            $scope.movieTips = {
                "movieTips": [
                    { "Title": "Django Unchained", "Year": "2012", "imdbID": "tt1853728", "Type": "movie" },
                    { "Title": "Django Unchained", "Year": "2012", "imdbID": "tt1853728", "Type": "movie" },
                    { "Title": "Hercules Unchained", "Year": "1959", "imdbID": "tt0052782", "Type": "movie" },
                    { "Title": "America Unchained", "Year": "2007", "imdbID": "tt1153075", "Type": "movie" },
                    { "Title": "Angel Unchained", "Year": "1970", "imdbID": "tt0065401", "Type": "movie" },
                    { "Title": "Unchained Memories: Readings from the Slave Narratives", "Year": "2003", "imdbID": "tt0343129", "Type": "movie" },
                    { "Title": "Women Unchained", "Year": "1974", "imdbID": "tt0204742", "Type": "movie" },
                    { "Title": "Conan Unchained: The Making of 'Conan'", "Year": "2000", "imdbID": "tt0430950", "Type": "movie" },
                    { "Title": "Unchained", "Year": "1955", "imdbID": "tt0048762", "Type": "movie" },
                    { "Title": "The Unchained Goddess", "Year": "1958", "imdbID": "tt0157135", "Type": "movie" }
                ]
            };
            console.log($scope.movieTips);
        };
    });