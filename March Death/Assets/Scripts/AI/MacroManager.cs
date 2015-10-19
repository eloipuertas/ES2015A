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
        AIController ai;
        public MacroManager(AIController ai)
        {
            this.ai = ai;
        }
        /// <summary>
        /// Called every few seconds, plans ahead and makes lists with what it wants
        /// </summary>
        public void MacroHigh()
        {
        }
        /// <summary>
        /// Called fast enough, acomplishes what the macroHigh asks for
        /// </summary>
        public void MacroLow()
        {

        }
        public void takeArms(int num)
        {
            if (ai.Workers.Count > 0)
            {
                int min = Math.Min(num, ai.Workers.Count);
                List<Unit> lu = ai.Workers.GetRange(0, min);
                ai.Workers.RemoveRange(0, min);
                ai.Army.AddRange(lu);
            }
        }
    }
}
