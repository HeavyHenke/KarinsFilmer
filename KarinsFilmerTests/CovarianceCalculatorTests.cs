using System;
using KarinsFilmer;
using KarinsFilmer.CouchDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KarinsFilmerTests
{
    [TestClass]
    public class CovarianceCalculatorTests
    {
        private SuggestionEngine _suggestionEngine;

        [TestInitialize]
        public void Setup()
        {
            CouchConfig.SetupCouchDb();
            _suggestionEngine = CreateSuggestionEngine();
        }


        [TestMethod]
        public void Dev_test()
        {
            using (new DurationsPrinter())
                _suggestionEngine.CalculateData();

            using (new DurationsPrinter())
                PrintSuggestionsFor("Lilian");

            PrintSuggestionsFor("Annelie");
            PrintSuggestionsFor("Karin");
            PrintSuggestionsFor("Mimmi");
            PrintSuggestionsFor("staffan.ekvall@gmail.com");
            PrintSuggestionsFor("Janne");
            PrintSuggestionsFor("Henrik");
        }

        private static SuggestionEngine CreateSuggestionEngine()
        {
            var repo = new CouchRepository();
            var linear = new LinearCovarianceCalculator(repo);
            var twoToOne = new TwoToOneCovarianceCalculator(repo);
            return new SuggestionEngine(linear, twoToOne, repo);
        }


        class DurationsPrinter : IDisposable
        {
            private readonly DateTime _start = DateTime.UtcNow;

            public void Dispose()
            {
                DateTime stop = DateTime.UtcNow;
                var duraction = (stop - _start).TotalMilliseconds;
                Console.WriteLine();
                Console.WriteLine($"Calculation time {duraction} ms");
            }
        }

        private void PrintSuggestionsFor(string user)
        {
            Console.WriteLine();
            Console.WriteLine("Suggestions for " + user);
            foreach (var movie in _suggestionEngine.GetSuggestionsForUser(user))
            {
                Console.WriteLine(movie.MovieTitle + "   " + movie.SuggestionWieght);
            }
        }
    }
}
