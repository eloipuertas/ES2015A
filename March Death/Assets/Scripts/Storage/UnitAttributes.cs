using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public sealed class UnitAttributes : EntityAttributes
    {
        public int weaponAbility;
        public int projectileAbility;
        public int strength;

        public float attackRange = 2.0f;
        public float attackRate;
        public float movementRate;

        public float foodConsumption;
        public float goldConsumption;
        public float goldProduction;
    }
}
