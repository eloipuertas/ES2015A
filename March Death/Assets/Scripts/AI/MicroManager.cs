using Assets.Scripts.AI.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Class tasked with mainly the control of the AI army.
/// this class will also control civils when they are either:
///     A: Exploring
///     B: Fighting for the motherland
/// </summary>
namespace Assets.Scripts.AI
{
    public class MicroManager
    {
		public const int AGENT_ATACK = 0;
		public const int AGENT_EXPLORER = 1;
		public const int AGENT_RETREAT = 2;
		public const int AGENT_ASSIST = 3;
        public const int AGENT_STRATEGY = 4;

        AIController ai;
        /// <summary>
        /// Commite of agents who will each vote at what to do with every squad
        /// </summary>
        public List<BaseAgent> agents;
        public List<Squad> squads;
        public MicroManager(AIController ai)
        {
            agents = new List<BaseAgent>();
            squads = new List<Squad>();
            this.ai = ai;
            AssistAgent assistAgent = new AssistAgent(ai, "Assist");
            AttackAgent aA = new AttackAgent(ai, assistAgent, "Atack");
            agents.Add(new ExplorerAgent(ai, assistAgent, "Explorer"));
            agents.Add(aA);
            agents.Add(new RetreatAgent(ai, aA, assistAgent, "Retreat"));
			agents.Add(assistAgent);
            agents.Add(new StrategyAgent(ai, assistAgent, "Strategy"));
            squads.Add(new Squad(ai.race,ai.DifficultyLvl)); //Hero
            squads.Add(new Squad(ai.race, ai.DifficultyLvl));
        }
        /// <summary>
        /// Called pretty fast, it's just like Update()
        /// </summary>
        public void Micro()
        {
            //difficulty == 0 means the AI is disabled
            if (ai.DifficultyLvl > 0)
            {
                foreach (BaseAgent agent in agents)
                {
                    agent.PreUpdate();
                }
                
                for(int i =0;i<squads.Count;i++)
                {
                    // Update squad
                    squads[i].Update();
                    float bVal = float.MinValue;
                    BaseAgent bAgent = agents[0];
                    int val;
                    foreach (BaseAgent a in agents)
                    {
                        val = a.getConfidence(squads[i]);
                        if (AIController.AI_DEBUG_ENABLED) ai.aiDebug.setAgentConfidence(a.agentName, val);
                        if (val > bVal)
                        {
                            bVal = val;
                            bAgent = a;
                        }
                    }

                    squads[i].UserData = bAgent;
                    if (AIController.AI_DEBUG_ENABLED)
                    {
                        ai.aiDebug.setControllingAgent(bAgent.agentName, bVal);
                    }

                    bAgent.controlUnits(squads[i]);

                    foreach (BaseAgent agent in agents)
                    {
                        agent.PostSquad();
                    }
                }

                foreach (BaseAgent agent in agents)
                {
                    agent.PostUpdate();
                }
            }
        }
        private void addSquad(List<Unit> units)
        {
            //Squad id 0 is used by temp squads
            Squad s = new Squad(ai.race,ai.DifficultyLvl);
            s.AddUnits(units);
            squads.Add(s);
        }
        public void setPersonality(List<float> rates)
        {
            if (rates.Count != agents.Count)
                Debug.LogError("setPersonality has different number of agents than personality rates");
            else
            {
                for (int i = 0; i < agents.Count; i++)
                    agents[i].modifier = rates[i];
            }
        }
        /// <summary>
        /// Boring personality, every agent has the same rate
        /// </summary>
        /// <param name="rate"></param>
        public void setPersonality(float rate)
        {
            for (int i = 0; i < agents.Count; i++)
                agents[i].modifier = rate;
        }

        /// <summary>
        /// Function tasked with deciding which squad should take care of the new created units
        /// </summary>
        /// <param name="u"></param>
        public void assignUnit(Unit u)
        {
            if (u.type == Storage.UnitTypes.HERO)
            {
                squads[0].AddUnit(u);
            }
            else if(u.type == Storage.UnitTypes.CIVIL)
            {
                Squad s = new Squad(ai.race,ai.DifficultyLvl);
                s.AddUnit(u);
                squads.Add(s);
            }
            else
            {
                Squad s = squads[squads.Count-1];
                if (s.Units.Count > 4)
                {
                    s = new Squad(ai.race, ai.DifficultyLvl);
                    squads.Add(s);
                }
                s.AddUnit(u);
            }
        }
    }
}