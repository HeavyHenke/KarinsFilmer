using Newtonsoft.Json;

namespace KarinsFilmer.CouchDb
{
    /// <summary>
    /// Temporary class used int Henriks database, should be replaced by MovieRating
    /// </summary>
    public class AllRatingsRow
    {
        public string Id { get; set; }
        public string Rev { get; set; }
        public string User { get; set; }
        [JsonProperty("imdbId")]
        public string ImdbId { get; set; }
        public int Rating { get; set; }
        public string Type { get; set; }
    }
}