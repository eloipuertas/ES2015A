using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storage
{
    /// <summary>
    /// Valid Races and Types for Resources.
    /// Might be expanded in a future
    ///
    /// <remarks>
    /// Should something be added, append it as the last element, otherwise
    /// previously assigned gameobjects might get wrong types
    /// </remarks>
    /// </summary>

    public class ResourceInfo : BuildingInfo
    {
        [JsonConverter(typeof(ResourceAttributesDataConverter))]
        public override EntityAttributes attributes { get; set; }

        [JsonConverter(typeof(ResourceActionsDataConverter))]
        public override List<EntityAbility> abilities { get; set; }

        public ResourceInfo()
        {
            abilities = new List<EntityAbility>();
        }

    }
}
