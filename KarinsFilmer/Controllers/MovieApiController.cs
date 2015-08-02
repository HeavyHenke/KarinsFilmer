using System.Collections.Generic;
using System.Web.Http;

namespace KarinsFilmer.Controllers
{
    public class MovieApiController : ApiController
    {
        [System.Web.Http.HttpGet]
        public List<MovieTip> GetMovieTips()
        {
            return new List<MovieTip> {
                new MovieTip { Title = "Django Unchained", Year = "2012", ImdbId = "tt1853728", Type = "movie" },
                new MovieTip {Title = "Hercules Unchained", Year = "1959", ImdbId = "tt0052782", Type = "movie" },
                new MovieTip {Title = "America Unchained", Year = "2007", ImdbId = "tt1153075", Type = "movie" },
                new MovieTip {Title = "Angel Unchained", Year = "1970", ImdbId = "tt0065401", Type = "movie" },
                new MovieTip {Title = "Unchained Memories: Readings from the Slave Narratives", Year = "2003", ImdbId = "tt0343129", Type = "movie" },
                new MovieTip {Title = "Women Unchained", Year = "1974", ImdbId = "tt0204742", Type = "movie" },
                new MovieTip {Title = "Conan Unchained: The Making of 'Conan'", Year = "2000", ImdbId = "tt0430950", Type = "movie" },
                new MovieTip {Title = "Unchained", Year = "1955", ImdbId = "tt0048762", Type = "movie" },
                new MovieTip {Title = "The Unchained Goddess", Year = "1958", ImdbId = "tt0157135", Type = "movie" }
                };
        }

        [System.Web.Http.HttpPost]
        public void SaveRating(string movieId, int rating)
        {

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
