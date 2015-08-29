using System.Web.Configuration;
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


        public override bool Equals(object obj)
        {
            return Equals((MovieInformationRow)obj);
        }

        public bool Equals(MovieInformationRow obj)
        {
            return ImdbId == obj.ImdbId;
        }

        public override int GetHashCode()
        {
            return ImdbId.GetHashCode();
        }
    }
}