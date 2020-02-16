using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CLOrdering
{
    public enum Temp
    {
        None,
        [EnumMember(Value ="hot")] Hot,
        [EnumMember(Value = "cold")] Cold,
        [EnumMember(Value = "frozen")] Frozen
    }

    public class Item
    {
        public Item()
        { }

        public Item(string name, Temp temperature, int shelfLife, double decayRate)
        {
            this.Name = name;
            this.Temperature = temperature;
            this.ShelfLife = shelfLife;
            this.DecayRate = decayRate;
        }

        public string Name { get; set; } = "";
        public Temp Temperature { get; set; } = Temp.None;
        public int ShelfLife { get; set; } = 0;
        public double DecayRate { get; set; } = 0.0;
    }
}
