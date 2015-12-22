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
        public float rangedAttackFurthest = 0;
        public float rangedAttackNearest = 0;
        public float projectileRadius = 0;
        public float projectileScale = 0;
        public int projectileSpeed = 0;
        public float projectileOffset = 1f;

        public float attackRate;
        public float movementRate = 1f;

        public float foodConsumption = 0.001f;
        public float goldConsumption;
        public float goldProduction;
    }
}
