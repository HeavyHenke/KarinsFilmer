using KarinsFilmer.CouchDb.Entities;
using MyCouch;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace KarinsFilmer.CouchDb
{
    public class CouchRepository
    {
        private readonly Uri _uri;

        public CouchRepository()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CouchDb"].ConnectionString;
            _uri = new Uri(connectionString);
        }

        /// <summary>
        /// Returns Id of created document
        /// </summary>
        private string CreateDocument(string document)
        {
            using (var c = new MyCouchStore(_uri))
            {
                var header = c.StoreAsync(document).Result;
                return header.Id;
            }
        }

        public void Add(CouchObject obj)
        {
            var document = JsonConvert.SerializeObject(obj);
            var id = CreateDocument(document);
            obj.Id = id;
        }

        public IEnumerable<MovieRating> GetAllMovieRatings()
        {
            using (var c = new MyCouchStore(_uri))
            {
                var query = new Query("views", "allRatings");
                query.Reduce = false;
                var result = c.QueryAsync<MovieRating>(query).Result;
                return result.Select(x => x.Value);
            }
        }
    }
}