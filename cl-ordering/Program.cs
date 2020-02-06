using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace cl_ordering
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonString = File.ReadAllText("Data/Samples.json");

            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            var menuItems = JsonSerializer.Deserialize<Item[]>(jsonString, options);

            OrderMaker OrderMaker = new OrderMaker(menuItems);

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
