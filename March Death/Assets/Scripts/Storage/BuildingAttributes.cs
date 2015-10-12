using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public sealed class BuildingAttributes : EntityAttributes
    {
        public EntityResources sellValue;
        
        public int timeToBuild;
        public float repairSpeed; // Auto repair speed

// resources only attributes

        public int storeSize;
        public int maxUnits;
        public int productionRate;
        public float updateInterval;

    }
}
