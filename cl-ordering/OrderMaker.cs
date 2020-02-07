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

        // With λ=3.25, there is a 90% chance the next order
        // comes within this many seconds
        public static readonly double nextOrderCap = .7084;
        readonly Item[] Items;
        Random Rng = new Random();

        public OrderMaker(Item[] items)
        {
            this.Items = items;
        }

        public delegate void NewOrder();
        public event EventHandler<EventArgs> PlaceOrderEvent;

        internal void Start()
        {
            PlaceOrderEvent = PlaceNextOrder;
            PlaceOrderEvent?.Invoke(this, new EventArgs());
        }

        async void PlaceNextOrder(object sender, EventArgs e)
        {
            double delay = ExpCdfInv(Rng.NextDouble());

            Console.WriteLine("Will place next order after " + delay + " seconds");

            await Task.Delay(Convert.ToInt32(delay*1000)).ConfigureAwait(false);
            PlaceOrderEvent?.Invoke(sender, new EventArgs());
        }

        // Inverse of 
        internal static double ExpCdfInv(double percentile)
        {
            percentile -= double.Epsilon;
            return Math.Log(1 - percentile) / -lambda;
        }
    }
}
