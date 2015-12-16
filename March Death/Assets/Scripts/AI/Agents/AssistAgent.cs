using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
	public class AssistAgent : BaseAgent
	{
        List<KeyValuePair<Squad, requestData>> requests;

        public AssistAgent(AIController ai, string name) : base(ai, name)
		{
            requests = new List<KeyValuePair<Squad, requestData>>();
		}
		
		public override void controlUnits(Squad squad)
		{
            Squad assisted = squad.AssistSquad;
            Vector2 cent = assisted.BoundingBox.Bounds.center;
            float y = (assisted.Units.Count>0)? assisted.Units[0].transform.position.y : 80f;
            Vector3 pos = new Vector3(cent.x, y, cent.y);
            Vector2 ocent = squad.BoundingBox.Bounds.center;
            if (Vector2.Distance(cent, ocent) < 30) //join squads
            {
                assisted.AddUnits(squad.Units);
                ai.Micro.squads.Remove(squad);
            }else
            {
                //Go to the most important request squad
                foreach (Unit u in squad.Units)
                {
                    u.moveTo(pos);
                    if (AIController.AI_DEBUG_ENABLED)
                    {
                        ai.aiDebug.registerDebugInfoAboutUnit(u, this.agentName);
                    }
                }
            }
		}
		
		public override int getConfidence(Squad squad)
		{
            if(requests.Count == 0)
            {
                return 0;
            }
            int bConfidence = int.MinValue;
            Squad bSquad = null;
            //Go to help the closest and more important request
            foreach(KeyValuePair<Squad, requestData> request in requests)
            {
                //I know they say you should help yourself before asking others, but I don't think this is what they mean.
                if(request.Key!= squad)
                {
                    int conf = request.Value.Priority;
                    float dist = Vector2.Distance(request.Key.BoundingBox.Bounds.center, squad.BoundingBox.Bounds.center);
                    if (dist > 200)
                    {
                        conf = conf / 4;
                    }
                    else if (dist > 100)
                    {
                        conf = conf / 2;
                    }
                    if (conf > bConfidence)
                    {
                        bConfidence = conf;
                        bSquad = request.Key;
                    }
                }
            }
            squad.AssistSquad = bSquad;
			return Mathf.Max(0,bConfidence);
		}

        public void requestHelp(Squad s, int priority)
        {
            int extra = 0;
            //Give extra priority to requests with the hero
            foreach (Unit u in s.Units)
            {
                if (u.type == Storage.UnitTypes.HERO)
                {
                    extra = 20;
                }
            }
            requestData r = new requestData(priority+extra);
            requests.Add(new KeyValuePair<Squad, requestData>(s,r));
        }

        public override void PreUpdate()
        {
            List<KeyValuePair<Squad, requestData>> toRemove = new List<KeyValuePair<Squad, requestData>>();
            foreach (KeyValuePair<Squad, requestData> request in requests)
            {
                if(request.Value.Life-- <= 0)
                {
                    toRemove.Add(request);
                }
            }
            if (toRemove.Count > 0)
            {
                //this may not be the nicest way to do this, feel free to change it if you know a better way.
                foreach (KeyValuePair<Squad, requestData> req in toRemove)
                {
                    requests.Remove(req);
                }
            }
        }
	}
    class requestData
    {
        public int Priority;
        /// <summary>
        /// Time until this request is purged
        /// </summary>
        public int Life;
        public requestData(int priority)
        {
            Priority = priority;
            Life = 3;
        }
    }
}