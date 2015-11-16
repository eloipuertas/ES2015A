using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
	public class AssistAgent : BaseAgent
	{
		Unit hero;
        KeyValuePair<SquadAI, int> mostImportantRequest;

		public AssistAgent(AIController ai, string name) : base(ai, name)
		{
			//Find our hero
			foreach (Unit u in ai.Army)
			{
				if (u.type == Storage.UnitTypes.HERO)
				{
					hero = u;
				}
			}
		}
		
		public override void controlUnits(SquadAI squad)
		{
            //Go to the most important request squad
            foreach(Unit u in squad.units)
            {
                u.moveTo(mostImportantRequest.Key.units[0].transform.position);
            }
		}
		
		
		public override int getConfidence(SquadAI squad)
		{	
			int confidence;
            confidence = extraConfidence;
			return confidence;
		}

        public void requestHelp(KeyValuePair<SquadAI, int> request)
        {
            if(request.Value > mostImportantRequest.Value)
            {
                mostImportantRequest = request;
            }
        }
	}
}