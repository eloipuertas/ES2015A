using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storage
{
    public class ArcheryInfo : BuildingInfo
    {

        [JsonConverter(typeof(ArcheryAttributesDataConverter))]
        public override EntityAttributes attributes { get; set; }

        [JsonConverter(typeof(ArcheryActionsDataConverter))]
        public override List<EntityAbility> abilities { get; set; }

        public ArcheryInfo()
        {
            abilities = new List<EntityAbility>();
        }

    }
}
