using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
    public class AttackAgent : BaseAgent
    {
		const int CONFIDENCE_NO_ENEMIES_AVALIABLE = 0;
		const int CONFIDENCE_OWN_SQUAD_SUPREMACY = 75;
		const int CONFIDENCE_OWN_SQUAD_SUPREMACI_MAX_MULTITPLIER = 5;

		float _maxUnitRange;
		Storage.Races _enemyRace;

        float valOfCitizen;
        public AttackAgent(AIController ai, string name) : base(ai, name)
        {
            valOfCitizen = 1f;
			if (ai.race == Storage.Races.ELVES)
			{
				_maxUnitRange = Storage.Info.get.of(Storage.Races.MEN, Storage.UnitTypes.THROWN).unitAttributes.rangedAttackFurthest;
				_enemyRace = Storage.Races.MEN;
			}
			else
			{
				_maxUnitRange = Storage.Info.get.of(Storage.Races.ELVES, Storage.UnitTypes.THROWN).unitAttributes.rangedAttackFurthest;
				_enemyRace = Storage.Races.ELVES;
			}
        }
        public override void controlUnits(SquadAI squad)
        {
            Vector3 Squadpos = Vector3.zero;
            if (squad.units.Count > 0)
            {
                //We assume our squad is mostly together 
                //TODO: Stop assuming members of the same squad are close
                Squadpos = squad.units[0].transform.position;
            }
            if (ai.EnemyUnits.Count > 0)
            {
                //Select target
                Unit bTar = ai.EnemyUnits[0];
                float bVal = float.MaxValue;
                foreach(Unit u in ai.EnemyUnits)
                {
                    float val = -Vector3.Distance(u.transform.position, Squadpos);
                    if (u.type == Storage.UnitTypes.HERO)
                        val += 10;
                    if (val < bVal)
                    {
                        bVal = val;
                        bTar = u;
                    }
                }

                foreach(Unit u in squad.units)
                {
                    if (u.status != EntityStatus.DEAD && !u.attackTarget(bTar))
                    {
                        u.moveTo(bTar.transform.position);
                        if(AIController.AI_DEBUG_ENABLED)
                        {
                            ai.aiDebug.registerDebugInfoAboutUnit(u, this.agentName);
                        }
                    }     
                }
                    
            }
        }
        
		/// <summary>
		/// Gets the confidence of this squad.
		/// </summary>
		/// <returns>The confidence.</returns>
		/// <param name="squad">Squad.</param>
        public override int getConfidence(SquadAI squad)
        {
            if (ai.EnemyUnits.Count == 0)
                return 0;

            float supremaciIndex = squad.getData<AttackData>().Value / squad.enemySquad.getData<AttackData>().Value;

            supremaciIndex = supremaciIndex == Mathf.Infinity ? 0 : supremaciIndex;

            Debug.Log("Supremaci index" + supremaciIndex);

            if (supremaciIndex > 0f)
            {
                return Mathf.RoundToInt(Mathf.Min(supremaciIndex, CONFIDENCE_OWN_SQUAD_SUPREMACI_MAX_MULTITPLIER) * CONFIDENCE_OWN_SQUAD_SUPREMACY);
            }
            else
            {
                return 0;
            }
            
        }
    }

    class AttackData : AgentData
    {
        public float Value { get; set; }
        public bool hasChanged { get; set; }
        public AttackData()
        {
            hasChanged = true;
        }
        public override void OnUnitJoined(Unit u)
        {
            hasChanged = true;
        }
        public override void OnUnitLeft(Unit u)
        {
            hasChanged = true;
        }
    }
}
