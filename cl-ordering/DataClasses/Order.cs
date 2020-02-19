using System;
using System.Diagnostics.CodeAnalysis;

namespace CLOrdering
{
    public class Order
    {
        internal readonly Item Item = new Item();
        internal readonly DateTime OrderTime = DateTime.Now;
        internal readonly int Id = 0;
        internal double Value = 0.0;
        internal DateTime Updated = DateTime.Now;

        internal static Order EmptyOrder = new Order();

        public Order()
        {
        }

        public Order(Item item)
        {
            this.Item = item;
            this.Value = this.Item.ShelfLife;
            Id = Convert.ToInt32(OrderTime.Ticks % 10000) + 1;  // ID 0 is reserved for EmptyOrder
        }

        public Order(Item item, DateTime dateTime):this(item)
        {
            this.OrderTime = dateTime;
        }

        public override string ToString()
        {
            return string.Format("#{0}: {1}", Id, Item.Name);
        }
    }
}
