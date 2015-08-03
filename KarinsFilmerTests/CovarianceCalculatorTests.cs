using System;
using KarinsFilmer;
using KarinsFilmer.CouchDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KarinsFilmerTests
{
    [TestClass]
    public class CovarianceCalculatorTests
    {
        private CovarianceCalculator _covarianceCalculator;

        [TestInitialize]
        public void Setup()
        {
            CouchConfig.SetupCouchDb();
            _covarianceCalculator = CreateCalculator();
        }


        [TestMethod]
        public void Dev_test()
        {
            using (new DurationsPrinter())
                _covarianceCalculator.CalculateData();

            using (new DurationsPrinter())
                PrintSuggestionsFor("Annelie");

            PrintSuggestionsFor("Karin");
            PrintSuggestionsFor("Mimmi");
            PrintSuggestionsFor("staffan.ekvall@gmail.com");
            PrintSuggestionsFor("Janne");
            PrintSuggestionsFor("Henrik");
        }

        private static CovarianceCalculator CreateCalculator()
        {
            return new CovarianceCalculator(new CouchRepository());
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
            foreach (var movie in _covarianceCalculator.GetSuggestionsForUser(user))
            {
                Console.WriteLine(movie.MovieTitle + "   " + movie.SuggestionWieght);
            }
        }
    }
}
