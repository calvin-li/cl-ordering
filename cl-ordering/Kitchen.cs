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
            internal readonly string Name;

            internal OrderCollection(int capacity, string name)
            {
                this.Capacity = capacity;
                this.Name = name;
            }

            internal bool HasRoom() => Count < Capacity;
        }

        internal Dictionary<Temp, OrderCollection> defaultShelves =
            new Dictionary<Temp, OrderCollection>(
                ((Temp[])Enum.GetValues(typeof(Temp)))
                .Where(x => x != Temp.None)
                .Select(x => new KeyValuePair<Temp, OrderCollection>(x, new OrderCollection(15, x.ToString()))));
        internal OrderCollection OverFLowShelf = new OrderCollection(20, "Overflow");
        internal OrderCollection WastedOrders = new OrderCollection(0, "Waste");

        public Kitchen()
        {
        }

        async internal void ReceiveOrder(object sender, OrderEventArgs o)
        {
            await Task.Run(() => ShelveOrder(o)).ConfigureAwait(false);
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
                //Console.WriteLine("Added new order: " + newOrder.Item.Name);

                DisplayShelves();
            }
            else
            {
                // Console.WriteLine("No shelf space");
            }
        }

        private void DisplayShelves()
        {
            int columnWidth = 54, i = 0;
            string clear = string.Join("", Enumerable.Repeat(' ', columnWidth));
            foreach (OrderCollection shelf in defaultShelves.Values.Append(OverFLowShelf))
            {
                int cursorX = i++ * columnWidth;
                int cursorY = 1;

                Console.SetCursorPosition(cursorX, cursorY++);
                Console.Write(string.Format("{0} Shelf ({1}):", shelf.Name, shelf.Count));

                // Copying orders to list to avoid race conditions
                Order[] orders = new Order[shelf.Count];
                shelf.CopyTo(orders);
                foreach (Order o in orders)
                {
                    string[] orderString = o.ToString().Split('\n');
                    foreach(string line in orderString)
                    {
                        Console.SetCursorPosition(cursorX, cursorY);
                        Console.Write(clear);

                        Console.SetCursorPosition(cursorX, cursorY++);
                        Console.Write(line);
                    }
                }
            }
        }
    }
}
