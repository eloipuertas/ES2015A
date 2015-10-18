using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.AI.Agents
{
    public class AttackAgent : BaseAgent
    {
        public AttackAgent(AIController ai) : base(ai) { }
        public override void controlUnits(List<Unit> units)
        {
            //Do nothing
        }

        public override int getConfidence(List<Unit> units)
        {
            return 21;
        }
    }
}
