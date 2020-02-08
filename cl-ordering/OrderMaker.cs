using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("UnitTests")]
namespace CLOrdering
{
    public class OrderMaker
    {
        public static readonly double lambda = 3.250;

        readonly Item[] Items;
        Random Rng = new Random();

        public OrderMaker(Item[] items)
        {
            this.Items = items;
        }

        public delegate void NewOrder();
        public event EventHandler<OrderEventArgs> PlaceOrderEvent;

        internal void Start()
        {
            PlaceOrderEvent += PlaceNextOrder;
            PlaceOrderEvent?.Invoke(this, CreateNewOrder());
        }

        private OrderEventArgs CreateNewOrder()
        {
            return new OrderEventArgs(this.Items[Rng.Next(Items.Length)]);
        }

        async void PlaceNextOrder(object sender, OrderEventArgs o)
        {
            double delay = ExpCdfInv(Rng.NextDouble());
            Console.WriteLine("Will place next order after " + delay + " seconds");

            await Task.Delay(Convert.ToInt32(delay*1000)).ConfigureAwait(false);
            PlaceOrderEvent?.Invoke(sender, CreateNewOrder());
        }

        // Given a percentile, returns an amount of seconds based on Poisson distribution
        // See README for more details
        internal static double ExpCdfInv(double percentile)
        {
            percentile -= double.Epsilon;
            return Math.Log(percentile) / -lambda;
        }
    }
}
