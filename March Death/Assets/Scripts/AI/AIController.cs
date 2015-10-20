using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;

/// <summary>
/// Class tasked with deciding what the enemy will do.
/// </summary>
namespace Assets.Scripts.AI 
{
    public class AIController : MonoBehaviour
    {
        public MacroManager Macro { get; set; }
        public MicroManager Micro { get; set; }

        /// <summary>
        /// Will be used to calculate the lvl of the AI
        /// </summary>
        public int DifficultyLvl { get; set; }

        List<AIModule> modules;
        //TODO: change this when decided about what do we really need to keep about buildings 
        public List<IGameEntity> OwnBuildings { get; set; }
        public List<IGameEntity> EnemyBuildings { get; set; }
        public List<Unit> EnemyUnits { get; set; }
        /// <summary>
        /// Just a basic way to keep track of what the enemy has more
        /// </summary>
        public Dictionary<UnitTypes,int> UnitsFound { get; set; }

        public List<Unit> Army { get; set; }
        public List<Unit> Workers { get; set; }

        float timer; //It's not going to overflow this millennium
        void Awake()
        {
            //Init lists
            Macro = new MacroManager(this);
            Micro = new MicroManager(this);
            EnemyUnits = new List<Unit>();
            EnemyBuildings = new List<IGameEntity>();
            modules = new List<AIModule>();
            Army = new List<Unit>();
            Workers = new List<Unit>();
            modules.Add(new AIModule(Macro.MacroHigh, 30f));
            modules.Add(new AIModule(Macro.MacroLow, 1f));
            modules.Add(new AIModule(Micro.Micro, 1f));
            timer = 0;

            ActorSelector selector = new ActorSelector()
            {
                registerCondition = gameObject => gameObject.GetComponent<FOWEntity>().IsOwnedByPlayer,
                fireCondition = gameObject => true
            };
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.DISCOVERED, OnEntityFound, selector);
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.HIDDEN, OnEntityLost, selector);
        }
        void Update()
        {
            timer += Time.deltaTime;
            foreach (AIModule m in modules)
                if (Math.Round(timer % m.period) == 0)
                    m.Callback();
        }
        void OnEntityFound(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isUnit)
                if (!EnemyUnits.Contains((Unit)g)) 
                    EnemyUnits.Add((Unit)g);
            else if (g.info.isBuilding)
                    if (!EnemyBuildings.Contains(g))
                        EnemyBuildings.Add(g);
        }
        void OnEntityLost(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isUnit)
                EnemyUnits.Remove((Unit)g);
            else if (g.info.isBuilding)
                EnemyBuildings.Remove(g);
        }
    }
    struct AIModule
    {
        public AIModule(Action Callback, float period) {
            this.Callback = Callback;
            this.period = period;
        }
        public Action Callback;
        /// <summary>
        /// Time in seconds between each call to Callback
        /// </summary>
        public float period;
    }
    
}
