using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KarinsFilmer.CouchDb.Entities
{
    public class MovieRating : CouchObject
    {
        public readonly string Type;
        public MovieRating()
        {
            Type = "MovieRating";
        }

        public string AccountId { get; set; }
        public int Rating { get; set; }
        public string MovieId { get; set; }

        public DateTime CreatationDate { get; set; }
    }
}