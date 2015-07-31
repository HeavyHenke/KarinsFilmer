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
        public string Movie { get; set; }
        public int Rating { get; set; }
        public string Type { get; set; }
    }
}