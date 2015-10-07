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

        public float attackRate;
        public float movementRate;
    }
}
