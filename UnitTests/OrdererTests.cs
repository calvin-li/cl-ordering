using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CLOrdering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class OrdererTests
    {
        [TestMethod]
        public void TestExp()
        {
            Random Rng = new Random();
            int iterations = 1000000, count = 0;
            double lambda = 3.250, nintieth = .7084, sum = 0;
            for(int i=0; i < iterations; i++)
            {
                double k = OrderMaker.ExpCdfInv(Rng.NextDouble(), lambda);
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

        [TestMethod]
        public void TestOrderData()
        {
            string jsonString = File.ReadAllText("Data/TestData.json");
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            var menuItems = JsonSerializer.Deserialize<Item[]>(jsonString, options);

            var count = new Dictionary<Item, int>(
                menuItems.Select(x => new KeyValuePair<Item, int>(x, 0)));

            Kitchen testKitchen = new Kitchen();
            double lambda = 10000;
            OrderMaker testOrderer = new OrderMaker(menuItems, lambda);
            testOrderer.OrderPlacedEvent +=
                (sender, args) =>
                {
                    Assert.IsTrue(Array.Exists(menuItems, x => x == args.Order.Item));
                    count[args.Order.Item] += 1;
                };

            testOrderer.Start();
            int wait = 1000;
            Task.Delay(wait).Wait();
            double total = count.Values.ToArray().Sum();
            foreach (Item item in menuItems) {
                Assert.IsTrue(count[item]/total < 0.35, "Percent wrong: " + count[item] / total);
            }
        }
    }
}
