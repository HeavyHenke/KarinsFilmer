using System.Collections.Generic;
using System.Linq;
using KarinsFilmer.CouchDb.Entities;

namespace KarinsFilmer.CouchDb
{
    class SuggestionEngine
    {
        private readonly LinearCovarianceCalculator _linearCovarianceCalculator;
        private readonly CouchRepository _couchRepository;

        private List<AllRatingsRow> _allRatings;
        private Dictionary<string, MovieInformationRow> _movieInformation;

        private bool HasCalculatedData { get; set; }


        public SuggestionEngine(LinearCovarianceCalculator linearCovarianceCalculator, CouchRepository couchRepository)
        {
            _couchRepository = couchRepository;
            _linearCovarianceCalculator = linearCovarianceCalculator;
        }

        public IList<MovieSuggestion> GetSuggestionsForUser(string userName)
        {
            CalculateData();

            var suggestions = new List<MovieSuggestion>();
            suggestions.AddRange(_linearCovarianceCalculator.SuggestionsForUser(userName));

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

            _linearCovarianceCalculator.CalculateData();

            _movieInformation = _couchRepository.GetAllMovieInformation().ToDictionary(key => key.ImdbId);
            _allRatings = _couchRepository.GetAllMovieRatings2().ToList();
        }
    }
}