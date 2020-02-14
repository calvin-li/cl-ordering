using System;

namespace CLOrdering
{
    public class OrderEventArgs: EventArgs
    {
        internal Order Order;

        public OrderEventArgs(Order order)
        {
            this.Order = order;
        }

        public OrderEventArgs(Item item):this(new Order(item)){ }
    }
}
