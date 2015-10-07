using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{


    public sealed class ResourceAttributes : EntityAttributes
    {
        public int weaponAbility;
        public int projectileAbility;
        public int strength;

        public float attackRate;
        public float movementRate;

        public int storeSize;
        public int maxUnits;
        public int productionRate;
        public float updateInterval;
    }
}
