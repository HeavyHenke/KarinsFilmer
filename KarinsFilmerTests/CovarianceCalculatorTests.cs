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
            CreateCalculator().CalculateData();
        }

        private static CovarianceCalculator CreateCalculator()
        {
            return new CovarianceCalculator(new CouchRepository());
        }
    }
}
