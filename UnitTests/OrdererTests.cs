using CLOrdering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class OrdererTests
    {
        [TestMethod]
        public void TestExp()
        {
            Random Rng = new Random();
            int iterations = 1000000, i = iterations, count = 0;
            double nintieth = .7084, sum = 0;
            while(i --> 1)
            {
                double k = OrderMaker.ExpCdfInv(Rng.NextDouble());
                if (k > nintieth)
                {
                    count += 1;
                }
                sum += k;
            }
            Assert.IsTrue(count > 99000 && count < 101000, 
                string.Format("Ninetieth percentile test failed: {0} ", count));

            double average = sum / iterations;
            Assert.IsTrue(average > 0.257 && average < .357, "failed average test: " + average);
        }
    }
}
