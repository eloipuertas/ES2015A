using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public sealed class ResourceAbility : EntityAbility
    {
        public int weaponAbilityModifier;
        public int projectileAbilityModifier;
        public int strengthModifier;
        public int resistanceModifier;
        public int woundsModifier;

        public float attackRateModifier;
        public float movementRateModifier;
    }
}