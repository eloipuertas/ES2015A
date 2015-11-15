using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storage
{
    public class StableInfo : BuildingInfo
    {

        [JsonConverter(typeof(StableAttributesDataConverter))]
        public override EntityAttributes attributes { get; set; }

        [JsonConverter(typeof(StableActionsDataConverter))]
        public override List<EntityAbility> abilities { get; set; }

        public StableInfo()
        {
            abilities = new List<EntityAbility>();
        }

    }
}
