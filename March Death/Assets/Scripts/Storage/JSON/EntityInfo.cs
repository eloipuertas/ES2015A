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
        public abstract List<EntityAbility> abilities { get; set; }

        public abstract T getType<T>() where T : struct, IConvertible;

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
                return isUnit && ((UnitInfo)this).type == UnitTypes.CIVIL;
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
        /// Returns true if the entity is a building, false otherwise
        /// </summary>
        public bool isBuilding
        {
            get
            {
                return entityType == EntityType.BUILDING;
            }
        }

        /// <summary>
        /// Returns true if the entity is a resource, false otherwise
        /// </summary>
        public bool isResource
        {
            get
            {
                return isBuilding && ((((BuildingInfo) this).type == BuildingTypes.FARM) || (((BuildingInfo) this).type == BuildingTypes.SAWMILL) || (((BuildingInfo) this).type == BuildingTypes.MINE));
            }
        }

        /// <summary>
        /// Returns true if the entity is a resource, false otherwise
        /// </summary>
        public bool isBarrack
        {
            get
            {
                return isBuilding && !isResource;
            }
        }

        /// <summary>
        /// If this info describes a unit, returns the UnitAttributes class, otherwise it returns null
        /// It should always be used either by first checking isUnit, or checking if returned value is not null
        /// </summary>
        public UnitAttributes unitAttributes
        {
            get
            {
                if (!isUnit)
                {
                    return null;
                }

                return (UnitAttributes)this.attributes;
            }
        }

        /// <summary>
        /// If this info describes a resource, returns the ResourceAttributes class, otherwise it returns null
        /// It should always be used either by first checking isResource, or checking if returned value is not null
        /// </summary>
        public ResourceAttributes resourceAttributes
        {
            get
            {
                if (!isResource)
                {
                    return null;
                }

                return (ResourceAttributes)this.attributes;
            }
        }

        /// <summary>
        /// If this info describes a building, returns the BuildingAttributes class, otherwise it returns null
        /// It should always be used either by first checking isBuilding, or checking if returned value is not null
        /// </summary>
        public BuildingAttributes buildingAttributes
        {
            get
            {
                if (!isBuilding)
                {
                    return null;
                }

                return (BuildingAttributes)this.attributes;
            }
        }


        /// <summary>
        /// If this info describes a barrack, returns the BarrackAttributes class, otherwise it returns null
        /// It should always be used either by first checking isBarrack, or checking if returned value is not null
        /// </summary>
        public BarrackAttributes barrackAttributes
        {
            get
            {
                if (!isBarrack)
                {
                    return null;
                }

                return (BarrackAttributes)this.attributes;
            }
        }

    }
}
