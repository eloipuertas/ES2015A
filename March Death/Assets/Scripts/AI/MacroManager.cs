using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Class tasked with handling the economy and unit creation of the AI.
/// MacroManager will handle:
///     Creating buildings, Getting upgrades, Creating units and distributing the work force (civil)
/// </summary>
namespace Assets.Scripts.AI
{
    public class MacroManager
    {
        
        /// <summary>
        /// This list contains what buildings the macro plans to build next
        /// </summary>
        List<BuildingTypes> buildingPref;
        List<UnitTypes> UnitPref;
        AIArchitect architect;
        AIController ai;
        public MacroManager(AIController ai)
        {
            this.ai = ai;
            buildingPref = new List<BuildingTypes>() { BuildingTypes.FARM, BuildingTypes.MINE, BuildingTypes.SAWMILL };
            UnitPref = new List<UnitTypes>() {UnitTypes.CIVIL};
            architect = new AIArchitect(ai);
        }
        /// <summary>
        /// Called every few seconds, plans ahead and makes lists with what it wants
        /// </summary>
        public void MacroHigh()
        {
            foreach (Resource r in ai.OwnResources)
                if (r.harvestUnits == 10) //TODO ask for the actual max
                    buildingPref.Add(r.type);
        }
        /// <summary>
        /// Called fast enough, acomplishes what the macroHigh asks for
        /// </summary>
        public void MacroLow()
        {
            if (buildingPref.Count > 0) //TODO and has resources
            {
                ai.CreateBuilding(buildingPref[0]);
                buildingPref.RemoveAt(0);
            }
            foreach(Resource r in ai.OwnResources)
            {
                if (r.harvestUnits < 10)
                    r.newCivilian();
            }
        }
        /// <summary>
        /// The micro is asking how many civils the army can spend (for exploring or defending)
        /// </summary>
        public int canTakeArms()
        {
            return ai.Workers.Count; 
        }
        /// <summary>
        /// The micro is forcibly taking num civils from the macro
        /// </summary>
        /// <param name="num"></param>
        public void takeArms(int num)
        {
            if (ai.Workers.Count > 0)
            {
                int min = Math.Min(num, ai.Workers.Count);
                List<Unit> lu = ai.Workers.GetRange(0, min);
                ai.Workers.RemoveRange(0, min);
                ai.addToArmy(lu);
            }
        }
    }
}
