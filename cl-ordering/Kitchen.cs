using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UnitTests")]
namespace CLOrdering
{
    public class Kitchen
    {
        internal class OrderCollection : ConcurrentDictionary<int, Order>
        {
            internal readonly int MaxOrders;
            internal readonly string Name;
            internal Order[] array;

            internal OrderCollection(int capacity, string name)
            {
                this.MaxOrders = capacity;
                this.Name = name;
                this.array = new Order[capacity];
                for (int i = 0; i < capacity; i++)
                {
                    this[i] = Order.EmptyOrder;
                }
            }

            internal bool HasRoom() => this.NumOrders() < MaxOrders;

            internal int NumOrders() => this.Count(x => x.Value != Order.EmptyOrder);
        }

        const int defaultShelfCapacity = 15;
        const int overFlowShelfCapacity = 20;

        internal static readonly Temp[] TempArray = (Temp[])Enum.GetValues(typeof(Temp));
        internal Dictionary<Temp, OrderCollection> DefaultShelves =
            new Dictionary<Temp, OrderCollection>(TempArray.Where(x => x != Temp.None)
                .Select(x => new KeyValuePair<Temp, OrderCollection>(
                    x, new OrderCollection(defaultShelfCapacity, x.ToString()))));
        internal OrderCollection OverFLowShelf = new OrderCollection(overFlowShelfCapacity, "Overflow");
        internal OrderCollection WastedOrders = new OrderCollection(0, "Waste");

        readonly Display Display;

        public Kitchen()
        {
            Display = new Display(DefaultShelves, OverFLowShelf);
            UpdateVal();
        }

        async internal void OnOrderReceived(object sender, OrderEventArgs o)
        {
            await Task.Run(() => ShelveOrder(o.Order)).ConfigureAwait(false);
        }

        internal void ShelveOrder(Order newOrder)
        {
            Temp orderTemp = newOrder.Item.Temperature;
            OrderCollection newOrderShelf = DefaultShelves[orderTemp].HasRoom() ?
                DefaultShelves[orderTemp] : OverFLowShelf.HasRoom() ?
                    OverFLowShelf : WastedOrders;

            if (newOrderShelf != WastedOrders)
            {
                int index = newOrderShelf.First(x => x.Value == Order.EmptyOrder).Key;
                newOrderShelf[index] = newOrder;
                //Console.WriteLine("Added new order: " + newOrder.Item.Name);

                bool overflow = newOrderShelf == OverFLowShelf;
                int column = GetColumn(newOrder, overflow);
                IEnumerable<string> newOrderString = Display.GetOrderString(newOrder, overflow);
                Display.AddToQueue(newOrder, index, column, newOrderString);
            }
            else
            {
                // Console.WriteLine("No shelf space");
            }
        }

        private async void UpdateVal()
        {
            while (true)
            {
                int j = 0;
                foreach (OrderCollection shelf in DefaultShelves.Values.Prepend(OverFLowShelf))
                {
                    for (int i = 0; i < shelf.MaxOrders; i++)
                    {
                        Order order = shelf[i];
                        if (order != Order.EmptyOrder)
                        {
                            UpdateVal(order, i, shelf == OverFLowShelf);
                        }
                    }
                    j += 1;
                }
                await Task.Delay(16).ConfigureAwait(false);
            }
        }

        private void UpdateVal(Order order, int index, bool overflow)
        {
            double age = (DateTime.Now - order.Updated).TotalSeconds;
            order.Value -= age * (1 + order.Item.DecayRate * (overflow ? 2 : 1));

            if (order.Value <= 0)
            {
                UnshelveOrder(order, index, overflow);
                if (!overflow)
                {
                    PromoteOrder(order.Item.Temperature);
                }
            }
            else
            {
                order.Updated = DateTime.Now;
            }
        }

        private void UnshelveOrder(Order order, int index, bool overflow)
        {
            if (overflow)
            {
                OverFLowShelf[index] = Order.EmptyOrder;
            }
            else
            {
                DefaultShelves[order.Item.Temperature][index] = Order.EmptyOrder;
            }
            Display.RemoveOrder(index, GetColumn(order, overflow));
        }

        private void PromoteOrder(Temp temp)
        {
            double minLife = double.PositiveInfinity;
            int minIndex = -1;
            for(int i=0; i<overFlowShelfCapacity; i++)
            {
                Order order = OverFLowShelf[i];
                if (order != Order.EmptyOrder && order.Item.Temperature == temp)
                {
                    double lifeLeft = order.Value / (1 + 2 * order.Item.DecayRate);
                    if (lifeLeft < minLife)
                    {
                        minLife = lifeLeft;
                        minIndex = i;
                    }
                }
            }

            if (minIndex >= 0)
            {
                ShelveOrder(OverFLowShelf[minIndex]);
                UnshelveOrder(OverFLowShelf[minIndex], minIndex, true);
            }
        }

        private static int GetColumn(Order order, bool overflow)
        {
            return overflow ? 0 : Array.IndexOf(TempArray, order.Item.Temperature);
        }
    }
}
