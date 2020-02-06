using System;
using System.Threading;

namespace cl_ordering
{
    public class OrderMaker
    {
        public static readonly double lambda = 3.25;

        // With λ=3.25, there is a 90% chance the next order
        // comes within this many seconds
        public static readonly double nextOrderCap = 7.48;

        Thread Orderer;

        public OrderMaker()
        {
        }
    }
}
