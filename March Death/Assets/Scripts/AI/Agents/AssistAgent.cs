using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
	public class AssistAgent : BaseAgent
	{
		Unit hero;
        List<KeyValuePair<Squad, int>> requests;
        KeyValuePair<Squad, int> mostImportantRequest;

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
            requests = new List<KeyValuePair<Squad, int>>();
		}
		
		public override void controlUnits(Squad squad)
		{
            //Go to the most important request squad
            foreach(Unit u in squad.Units)
            {
                u.moveTo(mostImportantRequest.Key.Units[0].transform.position);
            }
		}
		
		
		public override int getConfidence(Squad squad)
		{
            if (requests.Count == 0)
            {
                return 0;
            }

			int confidence;
            mostImportantRequest = requests[0];
            confidence = requests[0].Value;

            //Go to help the closest request
            foreach(KeyValuePair<Squad, int> request in requests)
            {
                if(Vector2.Distance(request.Key.BoundingBox.Bounds.center, squad.BoundingBox.Bounds.center) < 
                    Vector2.Distance(mostImportantRequest.Key.BoundingBox.Bounds.center, squad.BoundingBox.Bounds.center))
                {
                    mostImportantRequest = request;
                    confidence = request.Value;
                }   
            }

            confidence = extraConfidence;
			return confidence;
		}

        public void requestHelp(KeyValuePair<Squad, int> request)
        {
            extraConfidence = 0;
            requests.Add(request);
        }

        public override void PostSquad()
        {
            requests.Clear();
        }
	}
}