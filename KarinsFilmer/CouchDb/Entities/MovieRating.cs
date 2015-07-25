using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KarinsFilmer.CouchDb.Entities
{
    public class MovieRating
    {
        public const string Type = "MovieRating";

        [JsonIgnore] // Created by db
        public string Id { get; set; }

        public string AccountId { get; set; }
        public int Rating { get; set; }
        public string MovieId { get; set; }
    }
}