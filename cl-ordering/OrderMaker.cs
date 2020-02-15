using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UnitTests")]
namespace CLOrdering
{
    public class OrderMaker
    {
        private readonly double lambda = 3.250;

        readonly Item[] Items;
        readonly Random Rng = new Random();
        bool running = false;

        public event EventHandler<OrderEventArgs> OrderPlacedEvent;

        public OrderMaker(Item[] items)
        {
            this.Items = items;
        }

        // for testing
        internal OrderMaker(Item[] items, double lambda) : this(items)
        {
            this.lambda = lambda;
        }

        internal void Start()
        {
            running = true;
            PlaceOrders();
        }

        async void PlaceOrders()
        {
            while (running)
            {
                OrderEventArgs nextOrder = CreateNewOrder();
                //Console.WriteLine("Placing order: " + nextOrder.Order.Item.Name);
                OrderPlacedEvent?.Invoke(this, nextOrder);

                double delay = ExpCdfInv(Rng.NextDouble(), lambda);
                //Console.WriteLine("\tDelay: " + delay);
                await Task.Delay(Convert.ToInt32(delay * 1000)).ConfigureAwait(false);
            }

        }

        private OrderEventArgs CreateNewOrder()
        {
            return new OrderEventArgs(this.Items[Rng.Next(Items.Length)]);
        }

        // Given a percentile, returns an amount of seconds based on Poisson distribution
        // See README for more details
        internal static double ExpCdfInv(double percentile, double lambda)
        {
            percentile -= double.Epsilon;
            return Math.Log(percentile) / -lambda;
        }
    }
}
