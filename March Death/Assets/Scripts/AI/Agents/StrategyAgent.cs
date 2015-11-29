using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// This agent takes care of defending the base an attacking timers
//  it control units just sends people to sites, where other agents should take control of the unit
/// </summary>
namespace Assets.Scripts.AI.Agents
{
    public class StrategyAgent : BaseAgent
    {
        const int CONF_DEFEND = 45;
        const int CONF_ATTACK = 80;
        
        /// <summary>
        /// The army values when the strategy agent will start an attack
        /// In the case that the game didn't end with the last timing, the AI will repeatetly attack at that number
        /// </summary>
        Stack<int> timings;
        bool attacking;
        Vector3 targetPos;
        ///TODO: temp variable until we have patrolls
        Vector3 basePosition;
        System.Random rnd;
        public StrategyAgent(AIController ai, AssistAgent assist, string name) : base(ai, name)
        {
            rnd = new System.Random();
            //Until we have a better way to mix it we will just keep it random
            timings = new Stack<int>();
            timings.Push(rnd.Next(1300, 1700));
            timings.Push(rnd.Next(400, 600));
            if (rnd.Next(0, 9) < 3) //add a rush
                timings.Push(rnd.Next(60,100));
            basePosition = ai.rootBasePosition + new Vector3(50, 0, 50);
            targetPos = basePosition;
            attacking = false;
        }
        public override void controlUnits(SquadAI squad)
        {
            foreach(Unit u in squad.units)
            {
                if (Vector3.Distance(u.transform.position, targetPos) > 20)
                {
                    u.moveTo(targetPos);
                }
                if (AIController.AI_DEBUG_ENABLED)
                {
                    ai.aiDebug.registerDebugInfoAboutUnit(u, agentName);
                }
            }
        }

        public override int getConfidence(SquadAI squad)
        {
            return (attacking) ? CONF_ATTACK : CONF_DEFEND ;
        }
        /// <summary>
        /// This method decides what we are going to advocate for this loop.
        /// </summary>
        public void evaluateTimings()
        {
            float armyValue=0;
            foreach(SquadAI s in ai.Micro.squads)
            {
                armyValue += s.getData<AttackData>().Value;
            }
            if (armyValue > timings.Peek())
            {
                float EarmyValue = 0;
                //don't attack if what we can see from the other player overwhelms us
                foreach (SquadAI s in ai.Micro.squads)
                {
                    EarmyValue += s.enemySquad.getData<AttackData>().Value;
                }
                Debug.Log(EarmyValue);
                if(armyValue > (EarmyValue * 1.1))
                {
                    if(!attacking)
                    {
                        //If we aren't already attacking we find a target to attack 
                        attacking = true;
                        int eBuild = ai.EnemyBuildings.Count;
                        Debug.Log(eBuild);
                        if (eBuild > 0)
                        {
                            targetPos = ai.EnemyBuildings[rnd.Next(0, eBuild)].closestPointTo(ai.rootBasePosition);
                        }
                        else
                        {
                            //If we haven't found any enemy building we can't attack this time
                            RemovePush();
                        }
                    }
                }
                //We are attacking but the other army is bigger than ours
                else if (attacking) 
                {
                    RemovePush();
                }
            }       
        }
        public void RemovePush()
        {
            targetPos = basePosition;
            attacking = false;
            //If we have a bigger timer to attack we delete this timing, as it was clearly unsuccsefull
            if (timings.Count > 1)
                timings.Pop();
        }
    }
}
