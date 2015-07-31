using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

                foreach (var obj in result)
                {
                    string filename = "";

                    var doc = JsonConvert.DeserializeObject<JObject>(obj.IncludedDoc);

                    JToken type;
                    if (doc.TryGetValue("type", out type))
                        filename = type.ToString();
                    filename += "_" + obj.Key + ".json";
                    filename = filename.Replace("/", "_");

                    File.WriteAllText(filename, obj.IncludedDoc);
                }
            }
        }

        [TestMethod]
        [Ignore]
        public void ImportTestData()
        {
            var testDataResources = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(n => n.StartsWith("KarinsFilmerTests.TestData")).ToList();

            string connectionString = ConfigurationManager.ConnectionStrings["CouchDb"].ConnectionString;
            using (var c = new MyCouchStore(connectionString))
            {
                foreach (var resourceName in testDataResources)
                {
                    var jsonDoc = GetJsonDoc(resourceName);
                    c.StoreAsync(jsonDoc).Wait();
                }
            }
        }

        private string GetJsonDoc(string resourceName)
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (resourceStream == null)
                throw new Exception("Could not find testdata " + resourceName);

            var reader = new StreamReader(resourceStream, Encoding.UTF8, true, 1024, false);
            var stringFromFile = reader.ReadToEnd();
            return stringFromFile;
        }

    }
}
