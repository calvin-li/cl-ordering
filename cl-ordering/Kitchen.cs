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

        HashSet<Task> OrderUpdates = new HashSet<Task>();

        public event EventHandler<OrderEventArgs> OrderExpiredEvent;

        public Kitchen()
        {
            DrawHeaders();
            Update();
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

                OrderUpdates.Add(new Task( () => DrawOrder(newOrder, index, newOrderShelf == OverFLowShelf)));
            }
            else
            {
                // Console.WriteLine("No shelf space");
            }
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

        private void DrawOrder(Order order, int index, bool overflow)
        {
            int column = overflow ? 0 : Array.IndexOf(TempArray, order.Item.Temperature);
            int cursorX = column * columnWidth + leftHeaderSpace;
            int cursorY = index * rowHeight + topHeaderSpace;

            UpdateHeader(column);

            string orderString = order.ToString();
            if (overflow)
            {
                orderString += " (" + order.Item.Temperature.ToString()[0] + ")";
            }

            DisplayAtPos(cursorX, cursorY, clear);
            DisplayAtPos(cursorX, cursorY++, orderString);
            
            string progessbar = string.Join("", Enumerable.Repeat('=', progressWidth));
            DisplayAtPos(cursorX, cursorY, "[" + progessbar + "]" + order.Item.ShelfLife);
        }

        private async void Update()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    UpdateOrders();
                    UpdateProgress();
                    await Task.Delay(16).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        private void UpdateOrders()
        {
            Task[] updates = new Task[OrderUpdates.Count];
            OrderUpdates.CopyTo(updates);
            foreach (Task t in updates)
            {
                t.Start();
                t.Wait();
                OrderUpdates.Remove(t);
            }
        }

        private void UpdateHeader(int column)
        {
            int cursorY = 2;
            OrderCollection shelf = defaultShelves.Values.Prepend(OverFLowShelf).ToArray()[column];

            int cursorX = leftHeaderSpace + column * columnWidth + (shelf.Name + " Shelf (").Length;
            DisplayAtPos(cursorX, cursorY, "    ");
            DisplayAtPos(cursorX, cursorY, shelf.NumOrders() + "):");

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
                
                double age = (DateTime.Now - order.Updated).TotalSeconds;
                order.Value -= age * (1 + order.Item.DecayRate);
                order.Updated = DateTime.Now;
                

                double normVal = order.Value * 1.0 / order.Item.ShelfLife;
                int progress = Math.Max(Convert.ToInt32(Math.Ceiling(normVal * progressWidth)), 0);

                int cursorX = leftHeaderSpace + col * columnWidth + 1 + progress;
                int cursorY = topHeaderSpace + row * rowHeight + 1;

                string progressString = string.Join("", Enumerable.Repeat(" ", progressWidth - progress));
                if(normVal < 0.99 && progressString.Length < progressWidth)
                {
                    DisplayAtPos(cursorX-1, cursorY, "-");
                }

                int numDigits = 3;
                if (order.Value < 10)
                {
                    numDigits = 1;
                }
                else if (order.Value < 100)
                {
                    numDigits = 2;
                }

                if(progressString.Length >= numDigits + 1 && order.Value > 0)
                {
                    progressString = Convert.ToInt32(order.Value) + 
                        " " + progressString.Substring(numDigits + 1);
                    if(progress + progressString.Length > progressWidth)
                    {
                        progressString = progressString.Remove(progressString.Length - 1);
                    }
                }
                DisplayAtPos(cursorX, cursorY, progressString);
            }
        }

        private static void DisplayAtPos(int x, int y, string str)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(str);
        }
    }
}
