using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KarinsFilmer.CouchDb.Entities;

namespace KarinsFilmer.SuggestionEngine
{
    class LinearCovarianceCalculator
    {
        private CommonSuggestionEngineData _commonData;

        private Dictionary<Tuple<string, string>, double> _variance;

        private bool HasCalculatedData { get; set; }


        public async Task CalculateData(CommonSuggestionEngineData commonData)
        {
            if (HasCalculatedData)
                return;
            HasCalculatedData = true;

            _commonData = commonData;
            await Task.Run(() => DoCalculateData());
        }

        private void DoCalculateData()
        {
            List<string> movies = _commonData.MovieInformation.Keys.ToList();

            _variance = new Dictionary<Tuple<string, string>, double>();

            for (int i = 0; i < movies.Count - 1; i++)
            {
                for (int j = i + 1; j < movies.Count; j++)
                {
                    var val = CalculateCovariance(movies[i], movies[j]);
                    if (val < 0)
                        val = val/2;

                    if (val != 0)
                    {
                        _variance.Add(Tuple.Create(movies[i], movies[j]), val);
                        _variance.Add(Tuple.Create(movies[j], movies[i]), val);
                    }
                }
            }
        }


        public IEnumerable<MovieSuggestion> SuggestionsForUser(string user)
        {
            var suggestionsWithWeight = new List<Tuple<string, double>>();

            var scoreByUser = _commonData.AllRatings.Where(r => r.User == user).ToList();
            var moviesSeenByUser = scoreByUser.Select(r => r.ImdbId).ToList();
            var moviesNotSeenByUser = _commonData.MovieInformation.Keys.Except(moviesSeenByUser).ToList();

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

            return suggestionsWithWeight //.Where(c => c.Item2 >= 3)
                   .OrderByDescending(a => a.Item2)
                   .Select(imdbId => new MovieSuggestion(_commonData.MovieInformation[imdbId.Item1], imdbId.Item2));
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
            var byUser = _commonData.AllRatings.ToLookup(r => r.User);
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