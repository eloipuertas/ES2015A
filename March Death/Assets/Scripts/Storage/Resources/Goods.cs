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

        public float amount { get; set; }
        public WorldResources.Type type { get; set; }
    }
}

