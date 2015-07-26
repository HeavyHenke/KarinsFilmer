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

        public CovarianceCalculator(CouchRepository couchRepository)
        {
            _couchRepository = couchRepository;
        }

        public void CalculateData()
        {
            DateTime start = DateTime.UtcNow;

            ReadInformationFromDatabase();

            List<string> movies = _movieInformation.Keys.ToList();
            int num = 0;

            var variance = new Dictionary<Tuple<string, string>, double>();

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
                        variance.Add(Tuple.Create(movies[i], movies[j]), val);
                        variance.Add(Tuple.Create(movies[j], movies[i]), val);
                    }
                }
            }


            SuggestionsForUser(variance, "Karin");
            SuggestionsForUser(variance, "Mimmi");
            SuggestionsForUser(variance, "Staffan");
            SuggestionsForUser(variance, "Henrik");

            DateTime stop = DateTime.UtcNow;
            var duraction = (stop - start).TotalMilliseconds;
            Console.WriteLine();
            Console.WriteLine($"Calculation time {duraction} ms");
        }

        private void ReadInformationFromDatabase()
        {
            _movieInformation = _couchRepository.GetAllMovieInformation().ToDictionary(key => key.MovieTitle);
            _allRatings = _couchRepository.GetAllMovieRatings2().ToList();
        }

        private void SuggestionsForUser(Dictionary<Tuple<string, string>, double> variance, string user)
        {
            var scoreByHenrik = _allRatings.Where(r => r.User == user).ToList();
            var moviesSeenByHenrik = scoreByHenrik.Select(r => r.Movie).ToList();
            var moviesNotSeenByHenrik = _movieInformation.Keys.Except(moviesSeenByHenrik).ToList();

            Console.WriteLine();
            Console.WriteLine($"Movies not seen by {user}:");

            foreach (var movie in moviesNotSeenByHenrik)
            {
                var estimates = new List<double>();

                foreach (var s in scoreByHenrik)
                {
                    double v;
                    if (variance.TryGetValue(Tuple.Create(movie, s.Movie), out v))
                    {
                        var est = v * s.Rating;
                        est = (est + 5) / 2;
                        estimates.Add(est);
                    }
                }

                if (estimates.Count > 0)
                {
                    var estimateMean = estimates.Sum() / estimates.Count;
                    Console.WriteLine($"{movie}: {estimateMean}  ({estimates.Count})");
                }
            }
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