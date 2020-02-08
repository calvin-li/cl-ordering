using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CLOrdering
{
    class Program
    {
        static void Main()
        {
            string jsonString = File.ReadAllText("Data/Samples.json");

            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            var menuItems = JsonSerializer.Deserialize<Item[]>(jsonString, options);

            OrderMaker Orderer = new OrderMaker(menuItems);
            Orderer.Start();

            Task.Delay(-1).Wait();
        }
    }
}
