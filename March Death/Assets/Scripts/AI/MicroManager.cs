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
        AIController ai;
        /// <summary>
        /// Commite of agents who will each vote at what to do with every squad
        /// </summary>
        List<BaseAgent> agents;
        public MicroManager(AIController ai)
        {
            agents = new List<BaseAgent>();
            this.ai = ai;
            agents.Add(new ExplorerAgent(ai));
            AttackAgent aA = new AttackAgent(ai);
            agents.Add(aA);
            agents.Add(new RetreatAgent(ai, aA));
        }
        /// <summary>
        /// Called pretty fast, it's just like Update()
        /// </summary>
        public void Micro()
        {
            float bVal = float.MinValue;
            BaseAgent bAgent = agents[0];
            float val;
            foreach(List<Unit> lu in SplitInGroups(ai.Army))
            {
                foreach(BaseAgent a in agents)
                {
                    val = a.getConfidence(lu) * a.modifier + getError();
                    if (val > bVal)
                    {
                        bVal = val;
                        bAgent = a;
                    }
                }
                bAgent.controlUnits(lu);
            }
        }
        /// <summary>
        /// Outputs a different error based on the Ai difficulty level, only the easier difficulties should make errors
        /// </summary>
        /// <returns></returns>
        private float getError()
        {
            //TODO: improve when we can test it
            int error = Utils.D6.get.rollOnce();
            if (error > (ai.DifficultyLvl+3))
                return 6 / (ai.DifficultyLvl+1);
            return 0;
        }
        /// <summary>
        /// Splits units in different groups which will handled by different agents
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        private List<List<Unit>> SplitInGroups(List<Unit> units)
        {
            List<List<Unit>> squads= new List<List<Unit>>();
            //TODO, cluster by position or something faster
            squads.Add(units);
            return squads;
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
    }
}
