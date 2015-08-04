using System;
using System.Collections.Generic;
using System.Linq;
using KarinsFilmer.CouchDb;
using KarinsFilmer.CouchDb.Entities;

namespace KarinsFilmer.SuggestionEngine
{
    class TwoToOneCovarianceCalculator
    {
        private CommonSuggestionEngineData _commonData;
        private ILookup<string, AllRatingsRow> _ratingsByUser;
        private ILookup<KeyNode, ValueNode> _variance;

        private bool HasCalculatedData { get; set; }


        public void CalculateData(CommonSuggestionEngineData commonData)
        {
            if (HasCalculatedData)
                return;
            HasCalculatedData = true;

            _commonData = commonData;
            _ratingsByUser = _commonData.AllRatings.ToLookup(key => key.User);

            var variance = new List<Tuple<KeyNode, ValueNode>>();
            foreach (var combo in GetAllTwoMovieCombinations())
            {
                var valuesForKeyByMovie3 = GetAllKeyedValues(combo).ToLookup(key => key.Movie3);

                foreach (var v in valuesForKeyByMovie3)
                {
                    double score1 = v.Average(x => x.Rating1);
                    double score2 = v.Average(x => x.Rating2);
                    double score3 = v.Average(x => x.Rating3);
                    
                    var meanValueNode = new ValueNode(score1, score2, v.Key, score3);
                    variance.Add(Tuple.Create(combo, meanValueNode));
                }
            }

            _variance = variance.ToLookup(key => key.Item1, val => val.Item2);
        }

        private IEnumerable<ValueNode> GetAllKeyedValues(KeyNode combo)
        {
            foreach (var ratingsByUser in _ratingsByUser)
            {
                var allMoviesByUser = ratingsByUser.Select(r => r.ImdbId).ToList();
                if (!combo.ListContainsKey(allMoviesByUser))
                    continue; // User has not seen both movies.

                double score1 = ratingsByUser.First(r => r.ImdbId == combo.Movie1).Rating;
                double score2 = ratingsByUser.First(r => r.ImdbId == combo.Movie2).Rating;

                foreach (var rating in ratingsByUser)
                {
                    if(rating.ImdbId == combo.Movie1 || rating.ImdbId == combo.Movie2) continue;

                    yield return new ValueNode(score1, score2, rating.ImdbId, rating.Rating);
                }
            }
        }

        struct KeyNode
        {
            public string Movie1 { get; }
            public string Movie2 { get; }

            public KeyNode(string s1, string s2)
            {
                if (string.CompareOrdinal(s1, s2) > 0)
                {
                    Movie1 = s1;
                    Movie2 = s2;
                }
                else
                {
                    Movie1 = s2;
                    Movie2 = s1;
                }
            }

            public bool ListContainsKey(IList<string> list)
            {
                return list.Contains(Movie1) && list.Contains(Movie2);
            }
        }

        struct ValueNode
        {
            public double Rating1 { get; }
            public double Rating2 { get; }

            public string Movie3 { get; }
            public double Rating3 { get; }

            public ValueNode(double rating1, double rating2, string movie3, double rating3)
            {
                Rating1 = rating1;
                Rating2 = rating2;
                Movie3 = movie3;
                Rating3 = rating3;
            }
        }

        private IEnumerable<KeyNode> GetAllTwoMovieCombinations()
        {
            var allMovies = _commonData.MovieInformation.Keys.ToList();

            for (int i = 0; i < allMovies.Count - 1; i++)
            {
                for (int j = i + 1; j < allMovies.Count; j++)
                {
                    var movie1 = allMovies[i];
                    var movie2 = allMovies[j];

                    yield return new KeyNode(movie1, movie2);
                }
            }
        }


        public IEnumerable<MovieSuggestion> SuggestionsForUser(string user)
        {
            var scoreByUser = _commonData.AllRatings.Where(r => r.User == user).ToList();
            var moviesSeenByUser = new HashSet<string>(scoreByUser.Select(r => r.ImdbId));

            var suggestions = new List<Tuple<string, double, double>>(); // movie, estimated score, weight

            foreach (var combo in GetAllKeysByUser(user))
            {
                foreach (var val in _variance[combo])
                {
                    if (moviesSeenByUser.Contains(val.Movie3)) continue;

                    var userRating1 = _ratingsByUser[user].First(r => r.ImdbId == combo.Movie1).Rating;
                    var userRating2 = _ratingsByUser[user].First(r => r.ImdbId == combo.Movie2).Rating;

                    var distance = Math.Sqrt(Math.Pow((userRating1 - val.Rating1), 2) + Math.Pow((userRating2 - val.Rating2), 2));

                    const double maxDistance = 5.6568542494923801952067548968388;

                    double weight = maxDistance - distance;
                    double estimate = val.Rating3;

                    // If far apart, inverse the estimate and weight.
                    if (distance > maxDistance/2)
                    {
                        estimate = 6 - estimate;
                        weight = distance;
                    }

                    suggestions.Add(Tuple.Create(val.Movie3, estimate, weight));
                }
            }

            var suggestionByMovie = suggestions.ToLookup(key => key.Item1);
            var weightedAveragedSuggestions = suggestionByMovie.Select(x => Tuple.Create(x.Key, x.Sum(y => y.Item2*y.Item3) / x.Sum(z => z.Item3)));

            return weightedAveragedSuggestions //.Where(c => c.Item2 >= 3)
                   .OrderByDescending(a => a.Item2)
                   .Select(s => new MovieSuggestion(_commonData.MovieInformation[s.Item1], s.Item2));
        }

        private IEnumerable<KeyNode> GetAllKeysByUser(string user)
        {
            var userMovies = _ratingsByUser[user].Select(m => m.ImdbId).ToList();

            for (int i = 0; i < userMovies.Count - 1; i++)
            {
                for (int j = i + 1; j < userMovies.Count; j++)
                {
                    var movie1 = userMovies[i];
                    var movie2 = userMovies[j];

                    yield return new KeyNode(movie1, movie2);
                }
            }
        }

    }
}