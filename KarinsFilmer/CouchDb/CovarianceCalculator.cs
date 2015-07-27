using System;
using System.Collections.Generic;
using System.Linq;
using KarinsFilmer.CouchDb.Entities;

namespace KarinsFilmer.CouchDb
{
    class CovarianceCalculator
    {
        private readonly CouchRepository _couchRepository;
        private Dictionary<string, MovieInformationRow> _movieInformation;
        private List<AllRatingsRow> _allRatings;
        private Dictionary<Tuple<string, string>, double> _variance;

        public CovarianceCalculator(CouchRepository couchRepository)
        {
            _couchRepository = couchRepository;
        }

        public IList<string> GetSuggestionsForUser(string userName)
        {
            CalculateData();

            var suggestions = new List<string>();
            suggestions.AddRange(SuggestionsForUser(userName));
            if (suggestions.Count < 10)
            {
                var scoreByUser = _allRatings.Where(r => r.User == userName);
                var moviesSeenByUser = scoreByUser.Select(r => r.Movie);
                var excludeMovies = new HashSet<string>(_movieInformation.Keys.Except(moviesSeenByUser));
                foreach (var s in suggestions)
                    excludeMovies.Add(s);

                var additionalSuggestions = _movieInformation.Where(mi => excludeMovies.Contains(mi.Key) == false)
                    .OrderByDescending(m => m.Value.Count)
                    .ThenByDescending(m2 => m2.Value.Mean)
                    .Take(suggestions.Count - 10);
                
                suggestions.AddRange(additionalSuggestions.Select(m3 => m3.Key));
            }

            return suggestions;
        }

        public void CalculateData()
        {
            DateTime start = DateTime.UtcNow;

            ReadInformationFromDatabase();

            List<string> movies = _movieInformation.Keys.ToList();
            int num = 0;

            _variance = new Dictionary<Tuple<string, string>, double>();

            for (int i = 0; i < movies.Count - 2; i++)
            {
                for (int j = i + 1; j < movies.Count; j++)
                {
                    var val = CalculateCovariance(movies[i], movies[j]);
                    if (val < 0)
                        val = val / 2;

                    if (val != 0)
                    {
                        Console.WriteLine($"{movies[i]} - {movies[j]} => {val}");
                        _variance.Add(Tuple.Create(movies[i], movies[j]), val);
                        _variance.Add(Tuple.Create(movies[j], movies[i]), val);
                    }
                }
            }


            DateTime stop = DateTime.UtcNow;
            var duraction = (stop - start).TotalMilliseconds;
            Console.WriteLine();
            Console.WriteLine($"Calculation time {duraction} ms");
        }

        internal void PrintSuggestionsFor(string user)
        {
            foreach (var movie in GetSuggestionsForUser(user))
            {
                Console.WriteLine(movie);
            }
        }
            

        private void ReadInformationFromDatabase()
        {
            _movieInformation = _couchRepository.GetAllMovieInformation().ToDictionary(key => key.MovieTitle);
            _allRatings = _couchRepository.GetAllMovieRatings2().ToList();
        }

        private IEnumerable<string> SuggestionsForUser(string user)
        {
            var suggestionsWithWeight = new List<Tuple<string, double>>();

            var scoreByUser = _allRatings.Where(r => r.User == user).ToList();
            var moviesSeenByUser = scoreByUser.Select(r => r.Movie).ToList();
            var moviesNotSeenByUser = _movieInformation.Keys.Except(moviesSeenByUser).ToList();

            Console.WriteLine();
            Console.WriteLine($"Movies not seen by {user}:");

            foreach (var movie in moviesNotSeenByUser)
            {
                var estimates = new List<double>();

                foreach (var s in scoreByUser)
                {
                    double v;
                    if (_variance.TryGetValue(Tuple.Create(movie, s.Movie), out v))
                    {
                        var est = v * s.Rating;
                        est = (est + 5) / 2;
                        estimates.Add(est);
                    }
                }

                if (estimates.Count > 0)
                {
                    var estimateMean = estimates.Sum() / estimates.Count;
                    suggestionsWithWeight.Add(Tuple.Create(movie, estimateMean));
                }
            }

            return suggestionsWithWeight.Where(c => c.Item2 >= 3).OrderBy(a => a.Item2).Select(b => b.Item1);
        }

        private double CalculateCovariance(string movie1, string movie2)
        {
            var byUser = _allRatings.ToLookup(r => r.User);
            var samples = new List<Tuple<double, double>>();
            foreach (var u in byUser)
            {
                var m1 = u.FirstOrDefault(m => m.Movie == movie1);
                if (m1 == null) continue;

                var m2 = u.FirstOrDefault(m => m.Movie == movie2);
                if (m2 == null) continue;

                samples.Add(Tuple.Create((double)m1.Rating, (double)m2.Rating));
            }

            if (samples.Count < 2) return 0;

            double mean1 = (samples.Sum(x => x.Item1)) / samples.Count;
            double mean2 = (samples.Sum(x => x.Item2)) / samples.Count;

            var upper = samples.Sum(s => (s.Item1 - mean1) * (s.Item2 - mean2));
            var lower = Math.Sqrt(samples.Sum(s => (s.Item1 - mean1) * (s.Item1 - mean1)) * samples.Sum(s => (s.Item2 - mean2) * (s.Item2 - mean2)));

            if (lower == 0) return 0;
            return upper / lower;
        }

    }
}