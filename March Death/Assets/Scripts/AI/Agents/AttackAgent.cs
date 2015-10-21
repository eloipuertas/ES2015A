using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AI.Agents
{
    public class AttackAgent : BaseAgent
    {
        float valOfCitizen;
        public AttackAgent(AIController ai) : base(ai)
        {
            valOfCitizen = 1f;
        }
        public override void controlUnits(List<Unit> units)
        {
            Vector3 Squadpos = Vector3.zero;
            if (units.Count > 0)
            {
                //We assume our squad is mostly together 
                //TODO: Stop assuming members of the same squad are close
                Squadpos = units[0].transform.position;
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

                foreach(Unit u in units)
                    if (u.status != EntityStatus.DEAD && !u.attackTarget(bTar))
                        u.moveTo(bTar.transform.position);
            }
        }
        float valOfUnit(Unit u)
        {
            //TODO: Find a better formula with testing
            if (ai.DifficultyLvl < 5)
                return u.healthPercentage * u.info.unitAttributes.resistance+ u.info.unitAttributes.strength;
            else
                return u.healthPercentage * (u.info.unitAttributes.resistance + u.info.unitAttributes.attackRate * u.info.unitAttributes.strength);
        }
        public override int getConfidence(List<Unit> units)
        {
            if (ai.EnemyUnits.Count == 0)
                return 0;
            //TODO: Recalc our army value only when it changes
            float val = 0;
            foreach (Unit u in ai.Army)
                val += valOfUnit(u);
            foreach (Unit u in ai.EnemyUnits)
                val -= valOfUnit(u);
            if (val < 0)
            {
                int volunteers = ai.Macro.canTakeArms();
                float nval = (val + volunteers * valOfCitizen);
                if (nval > 0)
                {
                    ai.Macro.takeArms(volunteers);
                    return Mathf.RoundToInt(nval * 8);
                }
            }

            Debug.Log("Attack Agent Heuristic: " + Mathf.RoundToInt(val * 8));               
            return Mathf.RoundToInt(val*8);
        }
    }
}
