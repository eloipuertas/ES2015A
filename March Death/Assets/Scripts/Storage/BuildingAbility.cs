using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public sealed class BuildingAbility : EntityAction
    {
        public int resistanceModifier;
        public int woundsModifier;

        // resource building only modifiers

        // we can get better buildings. If be increese building level.
        // Best level resource buildings will store higher amount of materilas 
        // or hold more collection units and produce more materials.

        public int storeSizeModifier;  
        public int maxUnitsModifier;
        public int productionRateModifier;
    }
}
