using System;

namespace Storage
{
    /// <summary>
    /// Farms, sawMills and mines have production cycles.
    /// Each cycle new batch of materials are created. 
    /// this batchs are instances of class Goods.   
    /// </summary>
    
    public sealed class Goods
    {
        // constructor
        public Goods() { }

        public enum GoodsType { WOOD, METAL, FOOD };

        public float amount { get; set; }
        public GoodsType type { get; set; }

    }
}



