using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CLOrdering.Kitchen;

namespace CLOrdering
{
    class Display
    {
        private Dictionary<Temp, OrderCollection> DefaultShelves;
        internal OrderCollection OverFLowShelf;

        readonly int defaultShelfCapacity;
        readonly int overFlowShelfCapacity;

        internal ConcurrentQueue<Task> OrderUpdates = new ConcurrentQueue<Task>();

        const int columnWidth = 54;
        const int rowHeight = 3;
        const int leftHeaderSpace = 8;
        const int topHeaderSpace = 4;
        const int progressWidth = columnWidth - 12;
        static readonly string clear = string.Join("", Enumerable.Repeat(' ', columnWidth));

        internal Display(Dictionary<Temp, OrderCollection> defaultShelves, OrderCollection overflowShelf)
        {
            this.DefaultShelves = defaultShelves;
            this.OverFLowShelf = overflowShelf;

            this.defaultShelfCapacity = DefaultShelves.First().Value.MaxOrders;
            this.overFlowShelfCapacity = OverFLowShelf.MaxOrders;

            DrawHeaders();
            Update();
        }

        internal void RemoveOrder(int row, int column)
        {
            IEnumerable<string> emptyString = Enumerable.Repeat(clear, rowHeight-1);
            AddToQueue(Order.EmptyOrder, row, column, emptyString);
        }

        internal void AddToQueue(Order order, int row, int column, IEnumerable<string> orderString)
        {
            OrderUpdates.Enqueue(new Task(() => DrawOrder(row, column, orderString)));
        }

        internal static IEnumerable<string> GetOrderString(Order o, bool overflow)
        {
            string order = o.ToString();
            if (overflow)
            {
                order += " (" + o.Item.Temperature.ToString()[0] + ")";
            }

            return new string[]
            {
                order,
                "[" + string.Join("", Enumerable.Repeat('=', progressWidth-1)) + "] " + o.Item.ShelfLife
            };
        }

        private void DrawHeaders()
        {
            int i = 0;
            foreach (OrderCollection shelf in DefaultShelves.Values.Prepend(OverFLowShelf))
            {
                int cursorX = i++ * columnWidth + leftHeaderSpace;
                DisplayAtPos(cursorX, 2, string.Format("{0} Shelf ({1}):", shelf.Name, shelf.NumOrders()));
            }

            for (i = 0; i < Math.Max(defaultShelfCapacity, overFlowShelfCapacity); i++)
            {
                int cursorY = topHeaderSpace + i * rowHeight;
                DisplayAtPos(0, cursorY, (i + 1) + ".");
            }
        }

        private void DrawOrder(int row, int column, IEnumerable<string> orderString)
        {
            int cursorX = column * columnWidth + leftHeaderSpace;
            int cursorY = row * rowHeight + topHeaderSpace;

            UpdateHeader(column);
            foreach(string s in orderString)
            {
                DisplayAtPos(cursorX, cursorY++, s);
            }
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
            while (!OrderUpdates.IsEmpty)
            {
                Task t;
                if (OrderUpdates.TryDequeue(out t))
                {
                    t.Start();
                    t.Wait();
                }
            }
        }

        private void UpdateHeader(int column)
        {
            int cursorY = 2;
            OrderCollection shelf = DefaultShelves.Values.Prepend(OverFLowShelf).ToArray()[column];

            int cursorX = leftHeaderSpace + column * columnWidth + (shelf.Name + " Shelf (").Length;
            DisplayAtPos(cursorX, cursorY, "    ");
            DisplayAtPos(cursorX, cursorY, shelf.NumOrders() + "):");

        }

        private void UpdateProgress()
        {
            int j = 0;
            foreach (OrderCollection shelf in DefaultShelves.Values.Prepend(OverFLowShelf))
            {
                for (int i = 0; i < shelf.MaxOrders; i++)
                {
                    Order order = shelf[i];
                    if (order != Order.EmptyOrder)
                    {
                        UpdateProgress(order, i, j);
                    }
                }
                j += 1;
            }
        }

        private static void UpdateProgress(Order order, int row, int col)
        {
            double normVal = order.Value * 1.0 / order.Item.ShelfLife;
            int progress = Math.Max(Convert.ToInt32(Math.Ceiling(normVal * progressWidth)), 0);

            int cursorX = leftHeaderSpace + col * columnWidth + progress;
            int cursorY = topHeaderSpace + row * rowHeight + 1;

            string progressString = "] " + Convert.ToInt32(order.Value).ToString() + " ";
            DisplayAtPos(cursorX, cursorY, progressString);
        }

        private static void DisplayAtPos(int x, int y, string str)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(str);
        }
    }
}
