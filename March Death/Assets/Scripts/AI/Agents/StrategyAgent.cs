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
        float FIND_PLAYER_RATE = 60 * 5f;

        float lastSmell = 0f;
        
        /// <summary>
        /// The army values when the strategy agent will start an attack
        /// In the case that the game didn't end with the last timing, the AI will repeatetly attack at that number
        /// </summary>
        Stack<int> timings;
        bool attacking;
        IGameEntity target;
        List<Vector3> patrolPoints;
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
            patrolPoints = ai.Macro.architect.baseCriticPoints;
            attacking = false;
            FIND_PLAYER_RATE = FIND_PLAYER_RATE - 60 * ai.DifficultyLvl;
        }
        public override void controlUnits(Squad squad)
        {
            if (attacking)
            {
                if(target != null && target.status != EntityStatus.DESTROYED)
                {
                    try
                    {
                        squad.AttackTo(target);                 
                    }
                    catch(Exception ex)
                    {
                        target = null;
                    }
                }
                else
                {
                    findTarget();
                }
            }
            else
            {
                int posInd = squad.PatrolPosition;
                Vector3 targetPos = patrolPoints[posInd];
                Vector2 sc = squad.BoundingBox.Bounds.center;
                if (Vector3.Distance(new Vector3(sc.x, targetPos.y, sc.y), targetPos) < 5)
                {
                    //Got to the destination, let's go for the next one
                    if (posInd >= patrolPoints.Count-1)
                    {
                        posInd = 0;
                    }
                    else
                    {
                        posInd++;
                    }
                    targetPos = patrolPoints[posInd];
                    squad.PatrolPosition = posInd;
                }
                squad.MoveTo(targetPos);
            }
            if (AIController.AI_DEBUG_ENABLED)
            {
                foreach (Unit u in squad.Units)
                {
                    ai.aiDebug.registerDebugInfoAboutUnit(u, agentName);
                }                
            }
        }

        public override int getConfidence(Squad squad)
        {
            return (attacking) ? CONF_ATTACK : CONF_DEFEND ;
        }
        /// <summary>
        /// This method decides what we are going to advocate for this loop.
        /// </summary>
        public override void PreUpdate()
        {
            float armyValue=0;
            foreach(Squad s in ai.Micro.squads)
            {
                armyValue += s.Attack;
            }
            if (armyValue > timings.Peek())
            {
                float EarmyValue = 0;
                //don't attack if what we can see from the other player overwhelms us
                //TODO: this doesn't account for units on multiple squads
                foreach (Squad s in ai.Micro.squads)
                {
                    EarmyValue += s.EnemySquad.Attack;
                }
                if(armyValue > (EarmyValue * 1.1))
                {
                    if (!attacking)
                    {
                        //If we aren't already attacking we find a target to attack 
                        attacking = true;
                        findTarget();
                    }
                }
                //We are attacking but the other army is bigger than ours
                else if (attacking) 
                {
                    RemovePush();
                }
            }       
        }
        private void findTarget()
        {
            int eBuild = ai.EnemyBuildings.Count;
            IGameEntity stronghold = null;
            IGameEntity nearestTower = null;
            float minDistance = Mathf.Infinity;
            if (eBuild > 0)
            {
                foreach(IGameEntity building in ai.EnemyBuildings)
                {
                    if (building != null & building.status != EntityStatus.DESTROYED)
                    {
                        if (building.getType<Storage.BuildingTypes>() == Storage.BuildingTypes.WATCHTOWER)
                        {
                            nearestTower = building;
                            foreach (Squad s in ai.Micro.squads)
                            {   
                                try
                                {
                                    float dist = Vector3.Distance(s.BoundingBox.Bounds.center, building.getGameObject().transform.position);

                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        nearestTower = building;
                                    }
                                }
                                catch(Exception ex)
                                {
                                    continue;
                                }
 
                            }
                        }

                        else if (building.getType<Storage.BuildingTypes>() == Storage.BuildingTypes.STRONGHOLD)
                        {
                            stronghold = building;
                        }

                        else
                        {
                            foreach (Squad s in ai.Micro.squads)
                            {
                                try
                                {
                                    float dist = Vector3.Distance(s.BoundingBox.Bounds.center, building.getGameObject().transform.position);

                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        nearestTower = building;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                   
                }


                if (nearestTower != null)
                {
                    target = nearestTower;
                }

                else if (stronghold != null)
                {
                    target = stronghold;
                }
            }
            else
            {
                //If we haven't found any enemy building we can't attack this time
                if(FIND_PLAYER_RATE > Time.time && lastSmell + FIND_PLAYER_RATE > Time.time)
                {
                    Debug.Log(Time.time);
                    RemovePush();
                }
                else
                {
                    lastSmell = Time.time;
                    target = ai.race == Storage.Races.ELVES ? Helpers.getBuildingsOfRaceNearPosition(ai.Micro.squads[0].BoundingBox.Bounds.center, 2000f, Storage.Races.MEN)[0] : Helpers.getBuildingsOfRaceNearPosition(ai.Micro.squads[0].BoundingBox.Bounds.center, 800f, Storage.Races.ELVES)[0];
                    ai.EnemyBuildings.Add(target);
                }
            }
        }
        private void RemovePush()
        {
            attacking = false;
            //If we have a bigger timer to attack we delete this timing, as it was clearly unsuccsefull
            if (timings.Count > 1)
                timings.Pop();
        }
    }
}
