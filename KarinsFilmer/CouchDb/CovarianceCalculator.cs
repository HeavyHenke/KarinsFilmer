using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyCouch;

namespace KarinsFilmer.CouchDb
{
    class CovarianceCalculator
    {
        public void CalculateData()
        {
            DateTime start = DateTime.UtcNow;

            List<AllRatingsRow> rows;
            using (var db = new MyCouchClient("http://127.0.0.1:5984/karinsfilmer"))
            {
                using (var c = new MyCouchStore(db))
                {
                    var query = new Query("views", "allRatings");
                    query.Reduce = false;
                    rows = c.QueryAsync<AllRatingsRow>(query).Result.Select(r => r.Value).ToList();
                }
            }

            List<string> movies = rows.Select(r => r.Movie).Distinct().ToList();
            int num = 0;

            var variance = new Dictionary<Tuple<string, string>, double>();

            for (int i = 0; i < movies.Count - 2; i++)
            {
                for (int j = i + 1; j < movies.Count; j++)
                {
                    var val = CalculateCovariance(rows, movies[i], movies[j]);
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


            SuggestionsForUser(rows, movies, variance, "Karin");
            SuggestionsForUser(rows, movies, variance, "Mimmi");
            SuggestionsForUser(rows, movies, variance, "Staffan");
            SuggestionsForUser(rows, movies, variance, "Henrik");

            DateTime stop = DateTime.UtcNow;
            var duraction = (stop - start).TotalMilliseconds;
            Console.WriteLine();
            Console.WriteLine($"Calculation time {duraction} ms");
        }

        private static void SuggestionsForUser(List<AllRatingsRow> rows, List<string> movies, Dictionary<Tuple<string, string>, double> variance, string user)
        {
            var scoreByHenrik = rows.Where(r => r.User == user).ToList();
            var moviesSeenByHenrik = scoreByHenrik.Select(r => r.Movie).ToList();
            var moviesNotSeenByHenrik = movies.Except(moviesSeenByHenrik).ToList();


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

        private static double CalculateCovariance(List<AllRatingsRow> rows, string movie1, string movie2)
        {
            var byUser = rows.ToLookup(r => r.User);
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