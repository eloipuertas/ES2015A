using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storage
{
    public class BarrackInfo : BuildingInfo
    {

        [JsonConverter(typeof(BarrackAttributesDataConverter))]
        public override EntityAttributes attributes { get; set; }

        [JsonConverter(typeof(BarrackActionsDataConverter))]
        public override List<EntityAbility> abilities { get; set; }

        public BarrackInfo()
        {
            abilities = new List<EntityAbility>();
        }

    }
}
