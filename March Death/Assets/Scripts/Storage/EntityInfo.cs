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
        public abstract EntityAttributes attributes { get; set; }
        public abstract List<EntityAction> actions { get; set; }

        /// <summary>
        /// Returns true if the entity is a unit, false otherwise
        /// </summary>
        public bool isUnit
        {
            get
            {
                return entityType == EntityType.UNIT;
            }
        }

        /// <summary>
        /// Returns true if the unit is civil, false otherwise
        /// </summary>
        public bool isCivil
        {
            get
            {
                if (!isUnit)
                {
                    return false;
                }

                return toUnitInfo.type == UnitTypes.FARMER || toUnitInfo.type == UnitTypes.MINER || toUnitInfo.type == UnitTypes.LUMBERJACK;
            }
        }

        /// <summary>
        /// Returns true if the unit is of the army, false otherwise
        /// </summary>
        public bool isArmy
        {
            get
            {
                return isUnit && !isCivil;
            }
        }

        /// <summary>
        /// If this info describes a unit, returns the UnitInfo class, otherwise it returns false
        /// It should always be used either by first checking isUnit, or checking if returned value is not null
        /// </summary>
        public UnitInfo toUnitInfo
        {
            get
            {
                if (!isUnit)
                {
                    return null;
                }

                return (UnitInfo)this;
            }
        }
    }
}
