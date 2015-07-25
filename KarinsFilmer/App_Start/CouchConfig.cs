using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using MyCouch;
using MyCouch.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KarinsFilmer
{
    public static class CouchConfig
    {
        public static void SetupCouchDb()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CouchDb"].ConnectionString;
            var uri = new Uri(connectionString);

            using (var s = new MyCouchServerClient(uri.Scheme + "://" + uri.Authority))
            {
                var dbName = uri.LocalPath.Substring(1);
                var karinsFilmerDb = s.Databases.PutAsync(dbName).Result;

                EnsureViewsAreUpToDate(karinsFilmerDb);
            }
        }

        private static void EnsureViewsAreUpToDate(DatabaseHeaderResponse karinsFilmerDb)
        {
            using (var c = new MyCouchStore(karinsFilmerDb.RequestUri))
            {
                JObject viewJson = ReadViewFileFromSolution();
                string viewJsonString = viewJson.ToString(Formatting.None);

                string jsonFromDb = c.GetByIdAsync("_design/views").Result;
                if (jsonFromDb != null)
                {
                    var o = JsonConvert.DeserializeObject<JObject>(jsonFromDb);
                    o.Remove("_rev");
                    jsonFromDb = o.ToString(Formatting.None);
                }
                
                if (viewJsonString != jsonFromDb)
                {
                    c.SetAsync("_design/views", viewJsonString).Wait();
                }
            }
        }

        private static JObject ReadViewFileFromSolution()
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KarinsFilmer.CouchDb.views.json");
            var reader = new StreamReader(resourceStream, Encoding.UTF8, true, 1024, false);
            var stringFromFile = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<JObject>(stringFromFile);
        }

    }
}