using KarinsFilmer.CouchDb.Entities;
using MyCouch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace KarinsFilmer.CouchDb
{
    public class CouchRepository
    {
        /// <summary>
        /// Returns Id of created document
        /// </summary>
        private string CreateDocument(string document)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CouchDb"].ConnectionString;
            var uri = new Uri(connectionString);

            using (var c = new MyCouchStore(uri))
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
    }
}