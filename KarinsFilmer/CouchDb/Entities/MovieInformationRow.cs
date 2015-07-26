using Newtonsoft.Json;

namespace KarinsFilmer.CouchDb.Entities
{
    public class MovieInformationRow
    {
        public double StdDeviation  { get; set; }
        public int Count { get; set; }
        public double Mean { get; set; }
        public string MovieTitle { get; set; }
    }
}