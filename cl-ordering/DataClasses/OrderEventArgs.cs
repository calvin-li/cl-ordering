using System;

namespace CLOrdering
{
    public class OrderEventArgs: EventArgs
    {
        Order Order;

        public OrderEventArgs(Order order)
        {
            this.Order = order;
        }

        public OrderEventArgs(Item item)
        {
            this.Order = new Order(item);
        }
    }
}
