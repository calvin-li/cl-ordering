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
        internal class OrderCollection : List<Order>
        {
            internal readonly int MaxOrders;
            internal readonly string Name;

            internal OrderCollection(int capacity, string name)
            {
                this.MaxOrders = capacity;
                for (int i = 0; i < capacity; i++)
                {
                    this.Add(Order.EmptyOrder);
                }
                this.Name = name;
            }

            internal bool HasRoom() => this.NumOrders() < MaxOrders;

            internal int NumOrders() => this.Count(x => x != Order.EmptyOrder);
        }

        const int defaultShelfCapacity = 15;
        const int overFlowShelfCapacity = 20;

        static readonly Temp[] TempArray = (Temp[])Enum.GetValues(typeof(Temp));
        internal Dictionary<Temp, OrderCollection> defaultShelves =
            new Dictionary<Temp, OrderCollection>(TempArray.Where(x => x != Temp.None)
                .Select(x => new KeyValuePair<Temp, OrderCollection>(
                    x, new OrderCollection(defaultShelfCapacity, x.ToString()))));
        internal OrderCollection OverFLowShelf = new OrderCollection(overFlowShelfCapacity, "Overflow");
        internal OrderCollection WastedOrders = new OrderCollection(0, "Waste");

        const int columnWidth = 54;
        const int rowHeight = 3;
        const int leftHeaderSpace = 8;
        const int topHeaderSpace = 4;
        const int progressWidth = columnWidth - 12;
        static readonly string clear = string.Join("", Enumerable.Repeat(' ', columnWidth));

        public event EventHandler<OrderEventArgs> OrderExpiredEvent;

        public Kitchen()
        {
            DrawHeaders();
        }

        async internal void OnOrderReceived(object sender, OrderEventArgs o)
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
                int index = newOrderShelf.FindIndex(x => x == Order.EmptyOrder);
                newOrderShelf[index] = newOrder;
                //Console.WriteLine("Added new order: " + newOrder.Item.Name);

                DrawOrder(newOrder, index, newOrderShelf == OverFLowShelf);
            }
            else
            {
                // Console.WriteLine("No shelf space");
            }
            UpdateProgress();
        }

        private void DrawHeaders()
        {
            int i = 0;
            foreach (OrderCollection shelf in defaultShelves.Values.Prepend(OverFLowShelf))
            {
                int cursorX = i++ * columnWidth + leftHeaderSpace;
                DisplayAtPos(cursorX, 2, string.Format("{0} Shelf ({1}):", shelf.Name, shelf.NumOrders()));
            }

            for(i=0; i<Math.Max(defaultShelfCapacity, overFlowShelfCapacity); i++)
            {
                int cursorY = topHeaderSpace + i * rowHeight;
                DisplayAtPos(0, cursorY, (i+1) + ".");
            }
        }

        private static void DrawOrder(Order order, int index, bool overflow)
        {
            int column = overflow ? 0 : Array.IndexOf(TempArray, order.Item.Temperature);
            int cursorX = column * columnWidth + leftHeaderSpace;
            int cursorY = index * rowHeight + topHeaderSpace;

            string[] orderString = order == Order.EmptyOrder ?
                new string[] { "\n", "\n", "\n" } :
                order.ToString().Split('\n');
         
            foreach (string line in orderString)
            {
                DisplayAtPos(cursorX, cursorY, clear);
                DisplayAtPos(cursorX, cursorY++, line);
            }

            string progessbar = string.Join("", Enumerable.Repeat('=', progressWidth));
            DisplayAtPos(cursorX, cursorY, "[" + progessbar + "]" + order.Item.ShelfLife);
        }

        private void UpdateProgress()
        {
            int j = 0;
            foreach (OrderCollection shelf in defaultShelves.Values.Prepend(OverFLowShelf))
            {
                for (int i = 0; i < shelf.MaxOrders; i++)
                {
                    Order order = shelf[i];
                    if(order != Order.EmptyOrder)
                    {
                        UpdateProgress(order, i, j, shelf == OverFLowShelf);
                    }
                }
                j += 1;
            }
        }

        private static void UpdateProgress(Order order, int row, int col, bool overflow)
        {
            if (order.Value > 0)
            {
                int age = Convert.ToInt32((DateTime.Now - order.Updated).TotalSeconds);
                order.Value -= Convert.ToInt32(age * (1 + order.Item.DecayRate));
                order.Updated = DateTime.Now;

                double normVal = order.Value * 1.0 / order.Item.ShelfLife;
                int progress = Math.Max(Convert.ToInt32(normVal * progressWidth), 0);

                int cursorX = leftHeaderSpace + col * columnWidth + 1 + progress;
                int cursorY = topHeaderSpace + row * rowHeight + 1;

                string progressString = string.Join("", Enumerable.Repeat(" ", progressWidth - progress));
                DisplayAtPos(cursorX, cursorY, progressString);
            }
        }

        private static void DisplayAtPos(int x, int y, string str)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(str);
        }

        private void DrawOrders()
        {
            foreach (OrderCollection shelf in defaultShelves.Values.Prepend(OverFLowShelf))
            {
                DrawShelf(shelf);
            }
        }

        private void DrawShelf(OrderCollection shelf)
        {
            for(int i=0; i < shelf.MaxOrders; i++)
            {
                DrawOrder(shelf[i], i, shelf == OverFLowShelf);
            }
        }
    }
}
