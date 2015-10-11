using System.Collections.Generic;
using Newtonsoft.Json;

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

    public class UnitInfo : EntityInfo
    {
        public UnitTypes type = 0;

        [JsonConverter(typeof(UnitAttributesDataConverter))]
        public override EntityAttributes attributes { get; set; }

        [JsonConverter(typeof(UnitActionsDataConverter))]
        public override List<EntityAbility> actions { get; set; }

        public UnitInfo()
        {
            actions = new List<EntityAbility>();
        }
    }
}
