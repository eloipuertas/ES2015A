using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storage
{

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
