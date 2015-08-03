using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using KarinsFilmer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCouch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KarinsFilmerTests
{
    [TestClass]
    public class Tools
    {
        [TestMethod]
        [Ignore]
        public void ExportAllDataFromCouchToFiles()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CouchDb"].ConnectionString;
            using (var c = new MyCouchStore(connectionString))
            {
                var query = new Query("_all_docs") {IncludeDocs = true};
                var result = c.QueryAsync(query).Result;

                var jsonFileParts = new List<string>();

                foreach (var obj in result)
                {
                    var doc = JsonConvert.DeserializeObject<JObject>(obj.IncludedDoc);

                    JToken type;
                    if (doc.TryGetValue("type", out type) && type.ToString() == "MovieRating")
                        jsonFileParts.Add(obj.IncludedDoc);
                }

                var json = "[\n" + String.Join(",\n", jsonFileParts) + "\n]";
                File.WriteAllText("TestData.json", json);
            }
        }

        [TestMethod]
        [Ignore]
        public void ImportTestData()
        {
            // Remove the old database
            string connectionString = ConfigurationManager.ConnectionStrings["CouchDb"].ConnectionString;
            var uri = new Uri(connectionString);
            var dbName = uri.LocalPath.Substring(1);

            using (var s = new MyCouchServerClient(uri.Scheme + "://" + uri.Authority))
            {
                s.Databases.DeleteAsync(dbName).Wait();
            }

            // Create new database
            CouchConfig.SetupCouchDb();

            // Fill it with test data
            using (var c = new MyCouchStore(connectionString))
            {
                foreach (var testData in GetTestData())
                {
                    c.StoreAsync(testData).Wait();
                }
            }
        }

        private IEnumerable<string> GetTestData()
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KarinsFilmerTests.TestData.Testdata.json");
            if (resourceStream == null)
                throw new Exception("Could not find testdata ");

            var jsonArrayString = new StreamReader(resourceStream).ReadToEnd();
            var jsonArray = JsonConvert.DeserializeObject<JObject[]>(jsonArrayString);
            return jsonArray.Select(j => j.ToString());
        }
    }
}
