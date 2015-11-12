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
        private float _maxUnitRange;
        private Storage.Races _enemyRace;

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

        public override void controlUnits(List<Unit> units)
        {

            isHeroInDanger = false;

            // Mirar on estan els enemics
            enemySquadBoundingBox = getSquadBoundingBox(ai.EnemyUnits);
            
            // Mirar on estic jo
            ownSquadBoundingBox = getSquadBoundingBox(ai.Army);

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
                
                if(isHeroInDanger) attackAgent.controlUnits(squadToAtackManager);
            }
        }


        public override int getConfidence(List<Unit> units)
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
            ownSquadBoundingBox = getSquadBoundingBox(units);

            float maxLongitudeOfBox = ownSquadBoundingBox.width > ownSquadBoundingBox.height ? ownSquadBoundingBox.width : ownSquadBoundingBox.height;
            
            //Smell what is near this position
            Unit[] enemyUnitsNearUs = ai.senses.getUnitsOfRaceNearPosition(new Vector3(ownSquadBoundingBox.x, units[0].transform.position.y, ownSquadBoundingBox.y), maxLongitudeOfBox * 2 * _maxUnitRange, _enemyRace);

            foreach (Unit enemyUnit in enemyUnitsNearUs)
            {
                foreach (Unit ownUnit in units)
                {
                    float distance = Vector3.Distance(enemyUnit.transform.position, ownUnit.transform.position);
                    //HACK: Change this magic number before intefore integration.
                    if (distance < enemyUnit.currentAttackRange() + 5)
                    {
                        //If our hero is in range and is going to die
                        if(hero.healthPercentage < HERO_HEALTH_TOLERANCE_BEFORE_RETREAT && ownUnit.type == Storage.UnitTypes.HERO)
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
        /// Returns the bounding box of an squad
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        private Rect getSquadBoundingBox(List<Unit> units)
        {
            float minX = Mathf.Infinity;
            float maxX = -Mathf.Infinity;
            float minY = Mathf.Infinity;
            float maxY = -Mathf.Infinity;

            foreach (Unit u in units)
            {
                if (maxY < u.transform.position.z) maxY = u.transform.position.z;
                if (minY > u.transform.position.z) minY = u.transform.position.z;
                if (maxX < u.transform.position.x) maxX = u.transform.position.x;
                if (minX > u.transform.position.x) minX = u.transform.position.x;
            }

            return new Rect(minX, minY, (maxX - minX) * 2, (maxY - minY) * 2);
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
