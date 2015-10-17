using System;

namespace Storage
{
    // Constructor
    
    public sealed class Goods
    {
        // constructor
        public Goods()
        {
            
        }

        public enum GoodsType { WOOD, METAL, FOOD };

        public float amount { get; set; }
        public float gold { get; set; }
        public GoodsType type { get; set; }

    }
}



