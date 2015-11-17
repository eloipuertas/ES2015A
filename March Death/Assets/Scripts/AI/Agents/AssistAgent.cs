using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
	public class AssistAgent : BaseAgent
	{
		Unit hero;

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
		}
		
		
		public override int getConfidence(SquadAI squad)
		{	
			int confidence;
			confidence = useExtraConfidence();
			return confidence;
		}
	}
}