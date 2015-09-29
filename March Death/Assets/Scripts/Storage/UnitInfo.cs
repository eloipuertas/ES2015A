using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{

    /// <summary>
    /// Valid Races and Types for Units.
    /// Might be expanded in a future
    ///
    /// <remarks>
    /// Should something be added, append it as the last element, otherwise
    /// previously assigned gameobjects might get wrong types
    /// </remarks>
    /// </summary>
    public enum UnitTypes { FARMER, MINER, LUMBERJACK, HERO, LIGHT, HEAVY, THROWN, CAVALRY, MACHINE, SPECIAL };

    public sealed class UnitInfo : EntityInfo
    {
        public UnitTypes type = 0;

        public UnitAttributes attributes = null;
        public List<UnitAbility> abilities = new List<UnitAbility>();
    }
}
