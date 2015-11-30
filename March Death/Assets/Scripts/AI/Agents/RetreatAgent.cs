using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{

    public class RetreatAgent : BaseAgent
    {

        private const float HERO_HEALTH_TOLERANCE_BEFORE_RETREAT = 50f;
        private const float HERO_MIN_DISTANCE_WITH_ENEMIES_WHERE_IN_DANGER = 200f;
        private const float AI_MULTIPLIER_FACTOR = 8f;

        private const int CONFIDENCE_IN_ENEMY_ATACK_RANGE = 75;
        private const int CONFIDENCE_HERO_IS_AT_FIFTY_PERCENT = 1000;
        private const int CONFIDENCE_ASSIST_HELP_NEEDED = 400;

        AttackAgent attackAgent;
        AssistAgent assistAgent;

        Rect enemySquadBoundingBox, ownSquadBoundingBox;
        Vector3 safeArea;
        float minDistanceBetweenHeroAndNearestEnemy;

        private int confidence;

        bool isHeroInDanger;

        public RetreatAgent(AIController ai, AttackAgent aA, AssistAgent assist, string name) : base(ai, name)
        {
            attackAgent = aA;
            enemySquadBoundingBox = new Rect();
            ownSquadBoundingBox = new Rect();
            isHeroInDanger = false;
            minDistanceBetweenHeroAndNearestEnemy = 0f;

            assistAgent = assist;

        }

        public override void controlUnits(Squad squad)
        {
            // Mirar on estan els enemics
			enemySquadBoundingBox = squad.EnemySquad.BoundingBox.Bounds;
            
            // Mirar on estic jo
            ownSquadBoundingBox = squad.BoundingBox.Bounds;

            // Intentar Veure on hauria d'anar una unitat per estar protegida
            recalcSafePoint();

            if (ai.EnemyUnits.Count > 0)
            {
                foreach (Unit u in squad.Units)
                {
                    if (u.status != EntityStatus.DEAD)
                    {
                        u.moveTo(safeArea);
                    }

                    if (AIController.AI_DEBUG_ENABLED)
                    {
                        ai.aiDebug.registerDebugInfoAboutUnit(u, this.agentName);
                    }
                }
                assistAgent.addConfidence(CONFIDENCE_ASSIST_HELP_NEEDED);
                assistAgent.requestHelp(new KeyValuePair<Squad, int>(squad, CONFIDENCE_ASSIST_HELP_NEEDED));
            }
        }


        public override int getConfidence(Squad squad)
        {
            if (ai.EnemyUnits.Count == 0)
                return 0;
            
            minDistanceBetweenHeroAndNearestEnemy = 0;

            //Get the squad bounding box
            ownSquadBoundingBox = squad.BoundingBox.Bounds;

            foreach (Unit enemyUnit in squad.EnemySquad.Units)
            {
                foreach (Unit ownUnit in squad.Units)
                {
                    float distance = Vector3.Distance(enemyUnit.transform.position, ownUnit.transform.position);
                    //HACK: Change this magic number before intefore integration.
                    if (distance < enemyUnit.currentAttackRange() + 10)
                    {
                        //If our hero is in range and is going to die
                        if(ownUnit.type == Storage.UnitTypes.HERO && ownUnit.healthPercentage < HERO_HEALTH_TOLERANCE_BEFORE_RETREAT)
                        {
                            return CONFIDENCE_HERO_IS_AT_FIFTY_PERCENT;      
                        }

                        confidence = CONFIDENCE_IN_ENEMY_ATACK_RANGE;
                    }
                }
            }
             
            return confidence;
        }

        /// <summary>
        /// Tries to calculate a safe spot for non hero players in order to make rotations
        /// </summary>
        private void recalcSafePoint()
        {
            Vector2 esc = enemySquadBoundingBox.center;
            Vector2 osc = ownSquadBoundingBox.center;
            Vector2 safePointxzDirection = (esc - osc);
            safePointxzDirection.Normalize();
            safeArea = new Vector3(osc.x - safePointxzDirection.x * 30, ai.Army[0].transform.position.y, osc.y - safePointxzDirection.y * 30);
        }

    }
}
