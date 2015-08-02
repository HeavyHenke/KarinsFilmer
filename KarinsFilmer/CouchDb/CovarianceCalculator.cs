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

        private bool HasCalculatedData { get; set; }

        public CovarianceCalculator(CouchRepository couchRepository)
        {
            _couchRepository = couchRepository;
        }

        public IList<MovieInformationRow> GetSuggestionsForUser(string userName)
        {
            CalculateData();

            var suggestions = new List<MovieInformationRow>();
            suggestions.AddRange(SuggestionsForUser(userName));
            if (suggestions.Count < 10)
            {
                var moviesSeenByUser = _allRatings.Where(r => r.User == userName).Select(r => r.ImdbId).Distinct();
                var excludeMovies = new HashSet<string>(moviesSeenByUser);
                foreach (var s in suggestions)
                    excludeMovies.Add(s.ImdbId);

                var additionalSuggestions = _movieInformation.Values
                    .Where(mi => excludeMovies.Contains(mi.ImdbId) == false)
                    .OrderByDescending(m => m.Count)
                    .ThenByDescending(m2 => m2.Mean)
                    .Take(10 - suggestions.Count);
                
                suggestions.AddRange(additionalSuggestions);
            }

            return suggestions;
        }

        public void CalculateData()
        {
            if (HasCalculatedData)
                return;
            HasCalculatedData = true;

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
                        _variance.Add(Tuple.Create(movies[i], movies[j]), val);
                        _variance.Add(Tuple.Create(movies[j], movies[i]), val);
                    }
                }
            }
        }
           

        private void ReadInformationFromDatabase()
        {
            _movieInformation = _couchRepository.GetAllMovieInformation().ToDictionary(key => key.ImdbId);
            _allRatings = _couchRepository.GetAllMovieRatings2().ToList();
        }

        private IEnumerable<MovieInformationRow> SuggestionsForUser(string user)
        {
            var suggestionsWithWeight = new List<Tuple<string, double>>();

            var scoreByUser = _allRatings.Where(r => r.User == user).ToList();
            var moviesSeenByUser = scoreByUser.Select(r => r.ImdbId).ToList();
            var moviesNotSeenByUser = _movieInformation.Keys.Except(moviesSeenByUser).ToList();

            foreach (var movie in moviesNotSeenByUser)
            {
                var estimates = new List<double>();

                foreach (var s in scoreByUser)
                {
                    double v;
                    if (_variance.TryGetValue(Tuple.Create(movie, s.ImdbId), out v))
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

            return suggestionsWithWeight.Where(c => c.Item2 >= 3)
                   .OrderBy(a => a.Item2)
                   .Select(imdbId => _movieInformation[imdbId.Item1]);
        }

        private double CalculateCovariance(string movie1, string movie2)
        {
            var samples = GetSampelsWithBothMovies(movie1, movie2);

            if (samples.Count < 2) return 0;

            double mean1 = (samples.Sum(x => x.Item1)) / samples.Count;
            double mean2 = (samples.Sum(x => x.Item2)) / samples.Count;

            var upper = samples.Sum(s => (s.Item1 - mean1) * (s.Item2 - mean2));
            var lower = Math.Sqrt(samples.Sum(s => (s.Item1 - mean1) * (s.Item1 - mean1)) * samples.Sum(s => (s.Item2 - mean2) * (s.Item2 - mean2)));

            if (lower == 0) return 0;
            return upper / lower;
        }

        private List<Tuple<double, double>> GetSampelsWithBothMovies(string movie1, string movie2)
        {
            var byUser = _allRatings.ToLookup(r => r.User);
            var samples = new List<Tuple<double, double>>();
            foreach (var u in byUser)
            {
                var m1 = u.FirstOrDefault(m => m.ImdbId == movie1);
                if (m1 == null) continue;

                var m2 = u.FirstOrDefault(m => m.ImdbId == movie2);
                if (m2 == null) continue;

                samples.Add(Tuple.Create((double)m1.Rating, (double)m2.Rating));
            }
            return samples;
        }
    }
}