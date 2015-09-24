using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public enum Races { MEN, ELVES, DWARFS, LIZARDMEN, GREENSKINS, CHAOS, SKAVEN, UNDEAD, OGRES };
    public enum Types { FARMER, MINER, LUMBERJACK, HERO, LIGHT, HEAVY, THROWN, CAVALRY, MACHINE, SPECIAL };

    sealed class UnitInfo
    {

        public Races race = 0;
        public Types type = 0;

        public UnitAttributes attributes = null;
        public UnitResources resources = null;


        /// <summary>
        /// Returns true if the unit is civil, false otherwise
        /// </summary>
        public bool isCivil
        {
            get
            {
                return type == Types.FARMER || type == Types.MINER || type == Types.LUMBERJACK;
            }
        }

        /// <summary>
        /// Returns true if the unit is of the army, false otherwise
        /// </summary>
        public bool isArmy
        {
            get
            {
                return !isCivil;
            }
        }

    }
}
