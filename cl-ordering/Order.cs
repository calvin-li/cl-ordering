using System;
namespace CLOrdering
{
    public class Order
    {
        Item Item = new Item();
        DateTime OrderTime = DateTime.Now;
        Guid Id = new Guid();

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
