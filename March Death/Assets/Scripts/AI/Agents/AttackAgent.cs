using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
    public class AttackAgent : BaseAgent
    {
		const int CONFIDENCE_NO_ENEMIES_AVALIABLE = 0;
		const int CONFIDENCE_OWN_SQUAD_SUPREMACY = 75;
        const int CONFIDENCE_ENEMY_SQUAD_HAS_HERO = 500;
		const int CONFIDENCE_OWN_SQUAD_SUPREMACI_MAX_MULTITPLIER = 5;

		float _maxUnitRange;
		Storage.Races _enemyRace;

        int conf;
        float supremaciIndex;
        float valOfCitizen;

        AssistAgent assistAgent;

        public AttackAgent(AIController ai, AssistAgent assist, string name) : base(ai, name)
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

            assistAgent = assist;
        }

        public override void controlUnits(Squad squad)
        {
            Vector3 Squadpos = Vector3.zero;
            if (squad.Units.Count > 0)
            {
                //We assume our squad is mostly together 
                Squadpos = squad.BoundingBox.Bounds.center;
            }

            if (squad.EnemySquad.Units.Count > 0)
            {
                //Debug.Log(squad.Units[0] + " " + squad.EnemySquad.Units[0]);

                foreach (Unit u in squad.Units)
                {
                    //Select target
                    Unit bTar = null;
                    float bVal = float.MinValue;
                    foreach (Unit e in squad.EnemySquad.Units)
                    {
                        float val = -Vector3.Distance(u.transform.position, e.transform.position);
                        if (u.type == Storage.UnitTypes.HERO)
                            val += 80;
                        if (u.healthPercentage < 20)
                            val += 15;
                        if (val > bVal)
                        {
                            bVal = val;
                            bTar = e;
                        }
                    }
                    if (bTar!=null && bTar.status!=EntityStatus.DEAD && u.status != EntityStatus.DEAD && ((Unit)u.getTarget() != bTar))
                    {
                        u.attackTarget(bTar);
                    }
                    if (AIController.AI_DEBUG_ENABLED)
                    {
                        ai.aiDebug.registerDebugInfoAboutUnit(u, this.agentName);
                    }
                }
                    
            }
        }
        

		/// <summary>
		/// Gets the confidence of this squad.
		/// </summary>
		/// <returns>The confidence.</returns>
		/// <param name="squad">Squad.</param>
        public override int getConfidence(Squad squad)
        {
            if (squad.EnemySquad.Units.Count == 0)
                return 0;

            //Get the ratio of how better we are comparing us with the enemy army
            supremaciIndex = squad.Attack / squad.EnemySquad.Attack;

            //If is an infinity number we return 0
            supremaciIndex = supremaciIndex == Mathf.Infinity ? 0 : supremaciIndex;

            //Return the formula explained on the Issue max(n, 5) * 75
            if (supremaciIndex > 0f)
            {
                conf = Mathf.RoundToInt(Mathf.Min(supremaciIndex, CONFIDENCE_OWN_SQUAD_SUPREMACI_MAX_MULTITPLIER) * CONFIDENCE_OWN_SQUAD_SUPREMACY);

                //We need to check if the enemy squad has hero inside
                foreach (Unit u in squad.EnemySquad.Units)
                {
                    if(u.type == Storage.UnitTypes.HERO)
                    {
                        conf += CONFIDENCE_ENEMY_SQUAD_HAS_HERO;
                        break;
                    }
                }

                return conf;
            }

            return 0;

        }
    }
}
