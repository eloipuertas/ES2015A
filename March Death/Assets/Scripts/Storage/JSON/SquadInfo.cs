using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storage
{

    public class SquadInfo : EntityInfo
    {
        public override List<EntityAbility> abilities { get; set; }
        public override EntityAttributes attributes { get; set; }

        public override bool hasType() { return false; }
        public override T getType<T>() { throw new NotImplementedException(); }
    }
}
