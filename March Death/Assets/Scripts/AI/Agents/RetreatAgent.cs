using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{

    public class RetreatAgent : BaseAgent
    {

        private const float HERO_HEALTH_TOLERANCE_BEFORE_RETREAT = 40f;
        private const float AI_MULTIPLIER_FACTOR = 8f;
        private const int DEFCON1 = 10000;
        private const int DEFCON2 = 1000;
        private const int DEFCON3 = 100;
        private const int DEFCON4 = 10;
        private const int DEFCON5 = 1;

        float valOfCitizen;
        AttackAgent attackAgent;

        Rect enemySquadBoundingBox, ownSquadBoundingBox;
        Vector3 safeArea;
        float minDistanceBetweenHeroAndNearestEnemy;

        bool isHeroInDanger;

        public RetreatAgent(AIController ai, AttackAgent aA) : base(ai)
        {
            valOfCitizen = 1f;
            attackAgent = aA;
            enemySquadBoundingBox = new Rect();
            ownSquadBoundingBox = new Rect();
            isHeroInDanger = false;
            minDistanceBetweenHeroAndNearestEnemy = 0f;
        }

        public override void controlUnits(List<Unit> units)
        {

            /*
                TODO: 
                1 - Mirar on estan els enemics (Done)
                2 - Mirar on estem nosaltres (Done)
                3 - Intentar entendre com puc estar protegit (m....)
                4 - Si l'heroi s'esta morint intentar pasar l'atack agent (intentant estar aprop del heroi)
                i l'enemic (tot retirant-nos cap a la base).
                5 - Si l'heroi no s'esta morint intentar resguardar-me una mica enrrere.
            */
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
                    }
                    else
                    {
                        squadToAtackManager.Add(u);
                    }
                }
                
                if(isHeroInDanger) attackAgent.controlUnits(squadToAtackManager);
            }
        }


        public override int getConfidence(List<Unit> units)
        {
            if (ai.EnemyUnits.Count == 0)
                return 0;

            float val = 0;

            foreach (Unit u in ai.Army) {
                if (u.type == Storage.UnitTypes.HERO && u.healthPercentage < HERO_HEALTH_TOLERANCE_BEFORE_RETREAT)
                {
                    val += DEFCON1;
                }
            }
            return Mathf.RoundToInt(val * AI_MULTIPLIER_FACTOR);
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
            safeArea = new Vector3(ownSquadCenter.x - safePointxzDirection.x * 30, ai.Army[0].transform.position.y, ownSquadCenter.y - safePointxzDirection.y * 10);

        }
    }
}
