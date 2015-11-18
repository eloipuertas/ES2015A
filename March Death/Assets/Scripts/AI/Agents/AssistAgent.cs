using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
	public class AssistAgent : BaseAgent
	{
		Unit hero;
        List<KeyValuePair<SquadAI, int>> requests;
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
            requests = new List<KeyValuePair<SquadAI, int>>();
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
            if (requests.Count == 0)
            {
                return 0;
            }

			int confidence;
            mostImportantRequest = requests[0];
            confidence = requests[0].Value;

            //Go to help the closest request
            foreach(KeyValuePair<SquadAI, int> request in requests)
            {
                if(Vector2.Distance(request.Key.boudningBox.center, squad.boudningBox.center) < 
                    Vector2.Distance(mostImportantRequest.Key.boudningBox.center, squad.boudningBox.center))
                {
                    mostImportantRequest = request;
                    confidence = request.Value;
                }   
            }

            confidence = extraConfidence;
			return confidence;
		}

        public void requestHelp(KeyValuePair<SquadAI, int> request)
        {
            requests.Add(request);
        }

        public void clearRequests()
        {
            requests.Clear();
        }
	}
}