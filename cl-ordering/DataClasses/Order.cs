using System;
using System.Diagnostics.CodeAnalysis;

namespace CLOrdering
{
    public class Order: IComparable<Order>
    {
        internal readonly Item Item = new Item();
        internal readonly DateTime OrderTime = DateTime.Now;
        internal readonly Guid Id = Guid.NewGuid();
        internal double Value = 0;
        internal double Ttl = 0;

        public Order()
        {
        }

        public Order(Item item)
        {
            this.Item = item;
            this.Value = this.Item.ShelfLife;
        }

        public Order(Item item, DateTime dateTime)
        {
            this.Item = item;
            this.OrderTime = dateTime;
        }

        public int CompareTo([AllowNull] Order other)
        {
            return this.Value.CompareTo(other.Value);
        }

        public override string ToString()
        {
            return string.Format("\tid: {0}\n\t{1} / {2}\n",
                Id, Item, Item.ShelfLife / Value);
        }
    }
}
