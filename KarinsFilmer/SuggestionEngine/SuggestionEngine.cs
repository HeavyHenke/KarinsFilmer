using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KarinsFilmer.CouchDb;
using KarinsFilmer.CouchDb.Entities;

namespace KarinsFilmer.SuggestionEngine
{
    class SuggestionEngine
    {
        private readonly LinearCovarianceCalculator _linearCovarianceCalculator;
        private readonly TwoToOneCovarianceCalculator _twoToOneCovarianceCalculator;
        private readonly CouchRepository _couchRepository;

        private CommonSuggestionEngineData _commonData;

        private bool HasCalculatedData { get; set; }


        public SuggestionEngine(LinearCovarianceCalculator linearCovarianceCalculator, TwoToOneCovarianceCalculator twoToOneCovarianceCalculator, CouchRepository couchRepository)
        {
            _couchRepository = couchRepository;
            _linearCovarianceCalculator = linearCovarianceCalculator;
            _twoToOneCovarianceCalculator = twoToOneCovarianceCalculator;
        }

        public IList<MovieSuggestion> GetSuggestionsForUser(string userName)
        {
            CalculateData();

            var suggestions = new List<MovieSuggestion>();
            suggestions.AddRange(_twoToOneCovarianceCalculator.SuggestionsForUser(userName));

            // Add only suggestions from the one-level method that the two-level method did not find.
            var allreadySuggested = new HashSet<string>(suggestions.Select(s => s.ImdbId));
            suggestions.AddRange(_linearCovarianceCalculator.SuggestionsForUser(userName).Where(s => !allreadySuggested.Contains(s.ImdbId)));

            // Re-order the list
            suggestions = suggestions.OrderByDescending(s => s.SuggestionWieght).ToList();

            // Fill up with movies that needs rating.
            if (suggestions.Count < 10)
            {
                var moviesSeenByUser = _commonData.AllRatings.Where(r => r.User == userName).Select(r => r.ImdbId).Distinct();
                var excludeMovies = new HashSet<string>(moviesSeenByUser);
                foreach (var s in suggestions)
                    excludeMovies.Add(s.ImdbId);

                var additionalSuggestions = _commonData.MovieInformation.Values
                    .Where(mi => excludeMovies.Contains(mi.ImdbId) == false)
                    .OrderByDescending(m => m.Count)
                    .ThenByDescending(m2 => m2.Mean)
                    .Take(10 - suggestions.Count)
                    .Select(m => new MovieSuggestion(m, -m.Count));

                suggestions.AddRange(additionalSuggestions);
            }

            return suggestions;
        }


        public void CalculateData()
        {
            if (HasCalculatedData)
                return;
            HasCalculatedData = true;

            Task<IEnumerable<MovieInformationRow>> movieInformation = _couchRepository.GetAllMovieInformation();
            Task<IEnumerable<AllRatingsRow>> allRatings = _couchRepository.GetAllMovieRatings2();
            Task.WaitAll(movieInformation, allRatings);
            
            _commonData = new CommonSuggestionEngineData(allRatings.Result, movieInformation.Result);

            var calc1 = _linearCovarianceCalculator.CalculateData(_commonData);
            var calc2 = _twoToOneCovarianceCalculator.CalculateData(_commonData);
            Task.WaitAll(calc1, calc2);
        }
    }

    class CommonSuggestionEngineData
    {
        public List<AllRatingsRow> AllRatings { get; }
        public Dictionary<string, MovieInformationRow> MovieInformation { get; }

        public CommonSuggestionEngineData(IEnumerable<AllRatingsRow> allRatings, IEnumerable<MovieInformationRow> movieInformation)
        {
            MovieInformation = movieInformation.ToDictionary(key => key.ImdbId);
            AllRatings = allRatings.ToList();
        }
    }

}
