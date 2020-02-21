using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CLOrdering
{
    class Fleet
    {
        public static event EventHandler<OrderEventArgs> OrderPickupEvent;

        internal async static void OnOrderReceived(object sender, OrderEventArgs o)
        {
            await Task.Run(() => {
                Task.Delay(new Random().Next(2000, 10000)).Wait();
                OrderPickupEvent?.Invoke(sender, o);
            }).ConfigureAwait(false);
        }
    }
}
