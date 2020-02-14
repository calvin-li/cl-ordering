using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CLOrdering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class KitchenTests
    {
        static Item[] menuItems;
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            string jsonString = File.ReadAllText("Data/TestData.json");
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            menuItems = JsonSerializer.Deserialize<Item[]>(jsonString, options);
        }

        [TestMethod]
        public void OrderPlacementTest()
        {
            Kitchen testKitchen = new Kitchen();
            Order testOrder = new Order(menuItems[0]);

            testKitchen.ShelveOrder(new OrderEventArgs(testOrder));
            Temp testTemp = testOrder.Item.Temperature;
            Assert.IsTrue(testKitchen.defaultShelves[testTemp].Contains(testOrder));
        }
    }
}
