using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using KarinsFilmer.CouchDb;
using KarinsFilmer.SuggestionEngine;

namespace KarinsFilmer.Controllers
{
    public class MovieApiController : ApiController
    {
        [HttpGet]
        public List<MovieTip> GetMovieTips()
        {
            string user = HttpContext.Current.User.Identity.Name;
            var suggestions = CreateSuggestionEngine().GetSuggestionsForUser(user);

            return
                suggestions.Select(
                    s => new MovieTip {Title = s.MovieTitle, ImdbId = s.ImdbId, Year = s.MovieYear.ToString(), Type = "movie"})
                    .ToList();
        }

        [HttpPost]
        public void SaveRating(string imdbId, int rating)
        {

        }



        private static SuggestionEngine.SuggestionEngine CreateSuggestionEngine()
        {
            var repo = new CouchRepository();
            var linear = new LinearCovarianceCalculator();
            var twoToOne = new TwoToOneCovarianceCalculator();
            return new SuggestionEngine.SuggestionEngine(linear, twoToOne, repo);
        }

    }


    public class MovieTip
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string ImdbId { get; set; }
        public string Type { get; set; }
    }
}
