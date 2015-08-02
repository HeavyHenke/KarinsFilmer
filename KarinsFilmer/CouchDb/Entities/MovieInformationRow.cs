using Newtonsoft.Json;

namespace KarinsFilmer.CouchDb.Entities
{
    public class MovieInformationRow
    {
        public double StdDeviation  { get; set; }

        public int Count { get; set; }

        public double Mean { get; set; }

        [JsonProperty("movieTitle")]
        public string MovieTitle { get; set; }

        [JsonProperty("movieYear")]
        public int MovieYear { get; set; }

        [JsonProperty("imdbId")]
        public string ImdbId { get; set; }
    }
}