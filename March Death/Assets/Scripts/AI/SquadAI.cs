using Assets.Scripts.AI.Agents;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Groups units with data, used only by AI.
/// </summary>
namespace Assets.Scripts.AI
{
    public class SquadAI
    {

        const int ID_ENEMY_SQUAD = -1;

        public int Id { get; private set; }
        public List<Unit> units { get; private set; }
        /// <summary>
        /// Last agent that controlled this squad, can be null
        /// </summary>
        public BaseAgent lastAgent { get; set; }
        Dictionary<Type, AgentData> storage;

        public Rect boudningBox;
        public float squadValue;
        public SquadAI enemySquad = null;
        public List<IBuilding> enemyBuildings;

        float _maxUnitRange;
        Storage.Races _enemyRace;
        AIController ai;

        public SquadAI(int id, AIController ai)
        {
            Id = id;
            this.ai = ai;
            units = new List<Unit>();
            storage = new Dictionary<Type, AgentData>();

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
            units.Remove(u);

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

		/// <summary>
		/// Returns the bounding box of an squad
		/// </summary>
		/// <param name="units"></param>
		/// <returns></returns>
		public Rect getSquadBoundingBox()
		{
			float minX = Mathf.Infinity;
			float maxX = -Mathf.Infinity;
			float minY = Mathf.Infinity;
			float maxY = -Mathf.Infinity;
			
			foreach (Unit u in this.units)
			{
				if (maxY < u.transform.position.z) maxY = u.transform.position.z;
				if (minY > u.transform.position.z) minY = u.transform.position.z;
				if (maxX < u.transform.position.x) maxX = u.transform.position.x;
				if (minX > u.transform.position.x) minX = u.transform.position.x;
			}
			
			return new Rect(minX, minY, (maxX - minX) * 2, (maxY - minY) * 2);
		}

		public static Rect GetUnitListBoundingBox(List<Unit> targetUnits)
		{
			float minX = Mathf.Infinity;
			float maxX = -Mathf.Infinity;
			float minY = Mathf.Infinity;
			float maxY = -Mathf.Infinity;
			
			foreach (Unit u in targetUnits)
			{
				if (maxY < u.transform.position.z) maxY = u.transform.position.z;
				if (minY > u.transform.position.z) minY = u.transform.position.z;
				if (maxX < u.transform.position.x) maxX = u.transform.position.x;
				if (minX > u.transform.position.x) minX = u.transform.position.x;
			}
			
			return new Rect(minX, minY, (maxX - minX) * 2, (maxY - minY) * 2);
		}

        public void recalculateSquadValues()
        {

            if (units.Count == 0)
                return;
            //Recalculate the atack data of the squad    
            AttackData ad = getData<AttackData>();
            float val = 0;
            if (ad.hasChanged)
            {
                foreach (Unit u in this.units)
                {
                    val += valOfUnit(u);
                }
                ad.Value = val;
                ad.hasChanged = false;
            }
            else
            {
                val = ad.Value;
            }

            //calculate our own bounding box
            this.boudningBox = getSquadBoundingBox();
            
            //Calculate the EnemySquad values

            if(this.enemySquad == null)
            {
                enemySquad = new SquadAI(ID_ENEMY_SQUAD, ai);
            }

            float maxLongitudeOfBox = boudningBox.width > boudningBox.height ? boudningBox.width : boudningBox.height;

            //If we have only 1 unit we we need to change max longutude of the box to 1 because width of the rect is 0
            if (maxLongitudeOfBox < 1) maxLongitudeOfBox = 1f;

            //Smell what is near this position
            enemySquad.units = ai.senses.getVisibleUnitsOfRaceNearPosition(new Vector3(boudningBox.x, units[0].transform.position.y, boudningBox.y), maxLongitudeOfBox * 3 * _maxUnitRange, _enemyRace);

            //Get the enemy squad bounding box
            enemySquad.boudningBox = enemySquad.getSquadBoundingBox();

            //Calculate the Enemy units atack data
            ad = enemySquad.getData<AttackData>();
            val = 0;
  
            foreach (Unit u in enemySquad.units)
            {
                val += valOfUnit(u);
            }
            ad.Value = val;

            enemyBuildings = ai.senses.getBuildingsOfRaceNearPosition(new Vector3(boudningBox.x, units[0].transform.position.y, boudningBox.y), maxLongitudeOfBox * 3 * _maxUnitRange, _enemyRace);
        }

        float valOfUnit(Unit u)
        {
            return u.healthPercentage / 100 * (u.info.unitAttributes.resistance + u.info.unitAttributes.attackRate * u.info.unitAttributes.strength);
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
