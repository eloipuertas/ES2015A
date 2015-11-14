using Assets.Scripts.AI.Agents;
using System;
using System.Collections.Generic;

/// <summary>
/// Groups units with data, used only by AI.
/// </summary>
namespace Assets.Scripts.AI
{
    public class SquadAI
    {
        public int Id { get; private set; }
        public List<Unit> units { get; private set; }
        /// <summary>
        /// Last agent that controlled this squad, can be null
        /// </summary>
        public BaseAgent lastAgent { get; set; }
        Dictionary<Type, AgentData> storage;
        public SquadAI(int id)
        {
            Id = id;
            units = new List<Unit>();
            storage = new Dictionary<Type, AgentData>();
        }
        public void addUnit(Unit u)
        {
            units.Add(u);
            foreach (AgentData agent in storage.Values)
                if (agent != null)
                    agent.OnUnitJoined(u);
        }
        public void addUnits(List<Unit> group)
        {
            foreach(Unit u in group)
            {
                addUnit(u);
            }
        }
        public void removeUnit(Unit u)
        {
            units.Add(u);

            foreach (AgentData agent in storage.Values)
                if (agent != null)
                    agent.OnUnitLeft(u);
        }
        /// <summary>
        /// Retrieves the data associated to the BaseAgent agent or creates it if there's none
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="agent"></param>
        /// <returns></returns>
        public T getData<T>() where T : AgentData, new()
        {
            AgentData ad;
            //In the compiler we trust
            Type type = typeof(T);
            storage.TryGetValue(type, out ad);
            if (ad == null)
            {
                ad = new T();
                storage.Add(type, ad);
            }
            return (T)ad;
        }
    }
    /* Small class to save data for each agent on this squad
    Implements the following events:
        OnUnitLeft  (could leave for death or just reshuffling squads)
        OnUnitJoined

    Must have a nonParametized constructor
    */
    public class AgentData
    {
        public virtual void OnUnitLeft(Unit u) { }
        public virtual void OnUnitJoined(Unit u) { }
    }
}
