using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Dictionary<UnitTypes, int> UnitPref;
        AIController ai;
        public AIArchitect architect;
        static Dictionary<UnitTypes, BuildingTypes> unitToBuildMap = new Dictionary<UnitTypes, BuildingTypes>()
        {
            {UnitTypes.HEAVY,BuildingTypes.BARRACK},
            {UnitTypes.LIGHT,BuildingTypes.BARRACK},
            {UnitTypes.THROWN,BuildingTypes.ARCHERY}
        };
        public MacroManager(AIController ai)
        {
            this.ai = ai;

            UnitPref = new Dictionary<UnitTypes, int>();
            UnitPref.Add(UnitTypes.HEAVY, 5);
            UnitPref.Add(UnitTypes.LIGHT, 0);
            UnitPref.Add(UnitTypes.THROWN, 0);
            architect = new AIArchitect(ai);
        }
        /// <summary>
        /// Called every few seconds, plans ahead and makes lists with what it wants
        /// </summary>
        public void MacroHigh()
        {
            foreach (Resource r in ai.OwnResources)
                if (r.harvestUnits == r.maxHarvestUnits) 
                    architect.buildingPrefs.Add(r.type);
            List<UnitTypes> keys = new List<UnitTypes>(UnitPref.Keys);
            foreach (var key in keys)
            {
                int val = UnitPref[key] += 5;
                if (val < 0) //avoid overflows
                {
                    UnitPref[key] = 0;
                }
                else if (val > 100)
                {
                    if(architect.constructionGrid.mode == AIController.AIMode.BATTLE)
                    {
                        architect.buildingPrefs.Insert(0, unitToBuildMap[key]);
                    }
                }
            }
            if(architect.constructionGrid.mode == AIController.AIMode.CAMPAIGN)
            {
                architect.buildForCampaign();
            }
        }
        /// <summar>
        /// Called fast enough, acomplishes what the macroHigh asks for
        /// </summary>
        public void MacroLow()
        {
            if (architect.constructionGrid.mode == AIController.AIMode.BATTLE && architect.buildingPrefs.Count > 0)
            {
                architect.constructNextBuilding();
            }
            foreach (Resource r in ai.OwnResources)
            {
                if (r.harvestUnits < r.maxHarvestUnits)
                    r.newCivilian();
            }
            UnitTypes bUnit = (UnitPref.Aggregate((a, b) => a.Value > b.Value ? a : b)).Key;
            BuildingTypes needed = unitToBuildMap[bUnit];
            foreach (Barrack b in ai.OwnBarracks)
            {
                if (b.type == needed)
                {
                    UnitPref[bUnit] += (b.addUnitQueue(bUnit)) ? -1 : 0;
                }
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
                List<Unit> lu = new List<Unit>();

                int min = Math.Min(num, ai.Workers.Count);
                int edL = 0;
                while (lu.Count < min && edL < ai.OwnResources.Count)
                {
                    Resource building = ai.OwnResources[edL];
                    if (building.harvestUnits > 0)
                    {
                        Unit u = building.recruitExplorer();
                        if (u != null)
                        {
                            lu.Add(u);
                            ai.Workers.Remove(u);
                        }
                    }
                    else
                    {
                        edL++;
                    }
                    ai.addToArmy(lu);
                }
            }
        }
    }
}
