using System;
namespace CLOrdering
{
    public class Order
    {
        internal readonly Item Item = new Item();
        internal readonly DateTime OrderTime = DateTime.Now;
        internal readonly Guid Id = Guid.NewGuid();

        public Order()
        {
        }

        public Order(Item item)
        {
            this.Item = item;
        }

        public Order(Item item, DateTime dateTime)
        {
            this.Item = item;
            this.OrderTime = dateTime;
        }
    }
}
