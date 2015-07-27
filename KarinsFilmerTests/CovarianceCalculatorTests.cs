using KarinsFilmer;
using KarinsFilmer.CouchDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KarinsFilmerTests
{
    [TestClass]
    public class CovarianceCalculatorTests
    {
        [TestMethod]
        public void Dev_test()
        {
            CouchConfig.SetupCouchDb();
            var covarianceCalculator = CreateCalculator();

            covarianceCalculator.CalculateData();

            covarianceCalculator.PrintSuggestionsFor("Karin");
            covarianceCalculator.PrintSuggestionsFor("Mimmi");
            covarianceCalculator.PrintSuggestionsFor("Staffan");
            covarianceCalculator.PrintSuggestionsFor("Henrik");
        }

        private static CovarianceCalculator CreateCalculator()
        {
            return new CovarianceCalculator(new CouchRepository());
        }
    }
}
