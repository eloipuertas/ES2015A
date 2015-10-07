using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public sealed class UnitAbility : EntityAction
    {
        public int weaponAbilityModifier = 0;
        public int projectileAbilityModifier = 0;
        public int strengthModifier = 0;
        public int resistanceModifier = 0;
        public int woundsModifier = 0;

        public float attackRateModifier = 0;
        public float movementRateModifier = 0;
    }
}
