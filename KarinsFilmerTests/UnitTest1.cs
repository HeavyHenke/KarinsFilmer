using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KarinsFilmerTests
{
    [TestClass]
    public class CovarianceCalculatorTests
    {
        [TestMethod]
        public void Dev_test()
        {
            new KarinsFilmer.CouchDb.CovarianceCalculator().CalculateData();
        }
    }
}
