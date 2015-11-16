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

        Unit hero;

        AttackAgent attackAgent;

        Rect enemySquadBoundingBox, ownSquadBoundingBox;
        Vector3 safeArea;
        float minDistanceBetweenHeroAndNearestEnemy;

        private int confidence;

        bool isHeroInDanger;

        public RetreatAgent(AIController ai, AttackAgent aA, string name) : base(ai, name)
        {
            attackAgent = aA;
            enemySquadBoundingBox = new Rect();
            ownSquadBoundingBox = new Rect();
            isHeroInDanger = false;
            minDistanceBetweenHeroAndNearestEnemy = 0f;

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

            isHeroInDanger = false;

            // Mirar on estan els enemics
			enemySquadBoundingBox = SquadAI.GetUnitListBoundingBox(ai.EnemyUnits);
            
            // Mirar on estic jo
            ownSquadBoundingBox = squad.getSquadBoundingBox();

            // Intentar Veure on hauria d'anar una unitat per estar protegida
            recalcSafePoint();

            // A list to select who is going to atack
            List<Unit> squadToAtackManager = new List<Unit>();

            if (ai.EnemyUnits.Count > 0)
            {
                foreach (Unit u in ai.Army)
                {
                    if (u.status != EntityStatus.DEAD)
                    {
                        //If hero is going to die we need to do something
                        if(u.type == Storage.UnitTypes.HERO && u.healthPercentage < HERO_HEALTH_TOLERANCE_BEFORE_RETREAT)
                        {
                            u.moveTo(safeArea);
                            isHeroInDanger = true;
                        }
                        else
                        {
                            squadToAtackManager.Add(u);
                        }
                    }

                    if (AIController.AI_DEBUG_ENABLED)
                    {
                        ai.aiDebug.registerDebugInfoAboutUnit(u, this.agentName);
                    }
                }

                if (isHeroInDanger)
                {
                    SquadAI s = new SquadAI(0, ai);
                    s.addUnits(squadToAtackManager);
                    attackAgent.controlUnits(s);
                }
            }
        }


        public override int getConfidence(SquadAI squad)
        {
            if (ai.EnemyUnits.Count == 0)
                return 0;
            
            minDistanceBetweenHeroAndNearestEnemy = 0;
            
            //Calculate the min distance between enemies and our hero

            if(hero == null)
            {
                return 0;
            }

            //Get the squad bounding box
            ownSquadBoundingBox = squad.getSquadBoundingBox();

            foreach (Unit enemyUnit in squad.enemySquad.units)
            {
                foreach (Unit ownUnit in squad.units)
                {
                    float distance = Vector3.Distance(enemyUnit.transform.position, ownUnit.transform.position);
                    //HACK: Change this magic number before intefore integration.
                    if (distance < enemyUnit.currentAttackRange() + 5)
                    {
                        //If our hero is in range and is going to die
                        if(hero.healthPercentage < HERO_HEALTH_TOLERANCE_BEFORE_RETREAT && ownUnit.type == Storage.UnitTypes.HERO)
                        {
							ai.Micro.agents[MicroManager.AGENT_ASSIST].addConfidence(400);
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
            Vector2 enemySquadCenter = enemySquadBoundingBox.center;
            Vector2 ownSquadCenter = ownSquadBoundingBox.center;
            Vector2 safePointxzDirection = enemySquadCenter - ownSquadCenter;
            safeArea = ai.rootBasePosition;
            //safeArea = new Vector3(ownSquadCenter.x - safePointxzDirection.x * 30, ai.Army[0].transform.position.y, ownSquadCenter.y - safePointxzDirection.y * 10);

        }

    }
}
