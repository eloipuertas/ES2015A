using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
    public class ExplorerAgent : BaseAgent
    {
        public ExplorerAgent(AIController ai) : base(ai) { }
        public override void controlUnits(List<Unit> units)
        {
            //PLACEHOLDER, just go to the enemy base
            foreach (Unit u in units)
                u.moveTo(new UnityEngine.Vector3(806f, 120f, 167f));
        }

        public override int getConfidence(List<Unit> units)
        {
            return 20;
        }
    }
}
