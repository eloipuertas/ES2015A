using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public enum EntityType { UNIT, BUILDING, RESOURCE };
    public enum Races { MEN, ELVES, DWARFS, LIZARDMEN, GREENSKINS, CHAOS, SKAVEN, UNDEAD, OGRES };

    public abstract class EntityInfo
    {
        public EntityType entityType = EntityType.UNIT;
        public Races race = 0;
        public string name = "";

        public EntityResources resources;
    }
}
