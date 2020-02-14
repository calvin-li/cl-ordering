using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UnitTests")]
namespace CLOrdering
{
    public class Kitchen
    {
        internal class OrderCollection : SortedSet<Order>
        {
            int Capacity;

            internal OrderCollection(int capacity)
            {
                this.Capacity = capacity;
            }

            internal bool HasRoom() => Count < Capacity;
        }

        internal Dictionary<Temp, OrderCollection> defaultShelves =
            new Dictionary<Temp, OrderCollection>(
                ((Temp[])Enum.GetValues(typeof(Temp)))
                .Where(x => x != Temp.None)
                .Select(x => new KeyValuePair<Temp, OrderCollection>(x, new OrderCollection(15))));
        internal OrderCollection OverFLowShelf = new OrderCollection(20);
        internal OrderCollection WastedOrders = new OrderCollection(0);

        public Kitchen()
        {
        }

        async internal void ReceiveOrder(object sender, OrderEventArgs o)
        {
            await Task.Run(() => ShelveOrder(o));
        }

        internal void ShelveOrder(OrderEventArgs args)
        {
            Order newOrder = args.Order;
            Temp orderTemp = newOrder.Item.Temperature;
            OrderCollection newOrderShelf = defaultShelves[orderTemp].HasRoom() ?
                defaultShelves[orderTemp] : OverFLowShelf.HasRoom() ?
                    OverFLowShelf : WastedOrders;

            if (newOrderShelf != WastedOrders)
            {
                newOrderShelf.Add(newOrder);
                Console.WriteLine("Added new order: " + newOrder.Item.Name);
                Console.WriteLine("Current shelf items:");
                foreach (Order o in newOrderShelf)
                {
                    Console.WriteLine(string.Format(
                        "\tName: {0}\n\tId: {1}",
                        o.Item.Name, o.Id));
                }
            }
            else
            {
                Console.WriteLine("No shelf space");
            }
        }
    }
}
