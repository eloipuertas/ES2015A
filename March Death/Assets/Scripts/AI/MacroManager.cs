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
        Dictionary<UnitTypes, BuildingTypes> unitToBuildMap;
        public MacroManager(AIController ai)
        {
            this.ai = ai;

            UnitPref = new Dictionary<UnitTypes, int>();
            //Here is where we could use personalities, if they were implemented.
            List<int> randomPref = Shuffle<int>(new List<int>
            {
                80,100,85,97
            });
            UnitPref.Add(UnitTypes.HEAVY, randomPref[0]);
            UnitPref.Add(UnitTypes.LIGHT, randomPref[1]);
            UnitPref.Add(UnitTypes.THROWN, randomPref[2]);
            UnitPref.Add(UnitTypes.CAVALRY, randomPref[3]);
            UnitPref.Add(UnitTypes.MACHINE, 50);
            UnitPref.Add(UnitTypes.SPECIAL, 30);
            unitToBuildMap = new Dictionary<UnitTypes, BuildingTypes>()
                {
                    {UnitTypes.HEAVY,BuildingTypes.BARRACK},
                    {UnitTypes.LIGHT,BuildingTypes.BARRACK},
                    {UnitTypes.THROWN,BuildingTypes.ARCHERY},
                    {UnitTypes.CAVALRY,BuildingTypes.STABLE},
                };
            if (ai.race== Races.ELVES) //Goddammit, why couldn't this buildings have the same type
            {
                unitToBuildMap.Add(UnitTypes.SPECIAL,BuildingTypes.ENT);
                unitToBuildMap.Add(UnitTypes.MACHINE, BuildingTypes.WORKSHOP);
            }
            else
            {
                unitToBuildMap.Add(UnitTypes.SPECIAL, BuildingTypes.GRYPHON);
                unitToBuildMap.Add(UnitTypes.MACHINE, BuildingTypes.ARTILLERY);
            }
            architect = new AIArchitect(ai);
        }
        //Fisher-Yates
        public List<T> Shuffle<T>(List<T> l)
        {
            System.Random rng = new System.Random();
            int n = l.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T val = l[k];
                l[k] = l[n];
                l[n] = val;
            }
            return l;
        }
        /// <summary>
        /// Called every few seconds, plans ahead and makes lists with what it wants
        /// </summary>
        public void MacroHigh()
        {

            BuildArmyBuildings();
            BuildResourceBuildings();
            BuildDefences();
            if (architect.constructionGrid.mode == AIController.AIMode.CAMPAIGN)
            {
                architect.buildForCampaign();
            }
        }
        private void BuildDefences()
        {
            if (ResourcesPlacer.get(BasePlayer.ia).Amount(WorldResources.Type.WOOD) > 400)
            {
                for (int i = 0; i < (ai.DifficultyLvl * 3); i++)
                {
                    architect.addDefence();
                }
            }
        }
        private void BuildArmyBuildings()
        {
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
                    if (architect.constructionGrid.mode == AIController.AIMode.BATTLE)
                    {
                        architect.buildingPrefs.Insert(0, unitToBuildMap[key]);
                    }
                }
            }
        }
        /// <summary>
        /// Checks if our resource buildings are all almost full, and if they are then it adds another instance to be build
        /// </summary>
        private void BuildResourceBuildings()
        {
            List<BuildingTypes> toAdd = new List<BuildingTypes>()
            {
                BuildingTypes.FARM,
                BuildingTypes.MINE,
                BuildingTypes.SAWMILL
            };
            foreach (Resource r in ai.OwnResources)
            {
                if (r.harvestUnits < r.maxHarvestUnits - 1)
                {
                    toAdd.Remove(r.type);
                }
            }
            architect.buildingPrefs.AddRange(toAdd);
        }
        /// <summar>
        /// Called fast enough, acomplishes what the macroHigh asks for
        /// </summary>
        public void MacroLow()
        {
            if (architect.constructionGrid.mode == AIController.AIMode.BATTLE && architect.buildingPrefs.Count > 0 && ai.hasStronghold)
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
                    UnitPref[bUnit] += (b.addUnitQueue(bUnit)) ? -3 : 0;
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
