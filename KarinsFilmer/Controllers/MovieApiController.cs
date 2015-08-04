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

            //return new List<MovieTip> {
            //    new MovieTip { Title = "Django Unchained", Year = "2012", ImdbId = "tt1853728", Type = "movie" },
            //    new MovieTip {Title = "Hercules Unchained", Year = "1959", ImdbId = "tt0052782", Type = "movie" },
            //    new MovieTip {Title = "America Unchained", Year = "2007", ImdbId = "tt1153075", Type = "movie" },
            //    new MovieTip {Title = "Angel Unchained", Year = "1970", ImdbId = "tt0065401", Type = "movie" },
            //    new MovieTip {Title = "Unchained Memories: Readings from the Slave Narratives", Year = "2003", ImdbId = "tt0343129", Type = "movie" },
            //    new MovieTip {Title = "Women Unchained", Year = "1974", ImdbId = "tt0204742", Type = "movie" },
            //    new MovieTip {Title = "Conan Unchained: The Making of 'Conan'", Year = "2000", ImdbId = "tt0430950", Type = "movie" },
            //    new MovieTip {Title = "Unchained", Year = "1955", ImdbId = "tt0048762", Type = "movie" },
            //    new MovieTip {Title = "The Unchained Goddess", Year = "1958", ImdbId = "tt0157135", Type = "movie" }
            //    };
        }

        [HttpPost]
        public void SaveRating(string movieId, int rating)
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
