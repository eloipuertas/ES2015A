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
    public enum BuildingTypes { FORTRESS };

    public class BuildingInfo : EntityInfo
    {
        public BuildingTypes type = 0;

        [JsonConverter(typeof(BuildingAttributesDataConverter))]
        public override EntityAttributes attributes { get; set; }

        [JsonConverter(typeof(BuildingAttributesDataConverter))]
        public override List<EntityAbility> actions { get; set; }

        public BuildingInfo()
        {
            actions = new List<EntityAbility>();
        }
    }
}
