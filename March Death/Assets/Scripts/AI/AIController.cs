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
    public class AIController : BasePlayer
    {
        public const bool AI_DEBUG_ENABLED = true;

        public MacroManager Macro { get; set; }
        public MicroManager Micro { get; set; }
        public AIDebugSystem aiDebug;

        /// <summary>
        /// Will be used to calculate the lvl of the AI
        /// </summary>
        public int DifficultyLvl { get; set; }

        List<AIModule> modules;
        float[] timers;
        //TODO: change this when decided about what do we really need to keep about buildings 
        public List<IGameEntity> OwnBuildings { get; set; }
        public List<IGameEntity> EnemyBuildings { get; set; }
        public List<Unit> EnemyUnits { get; set; }
        /// <summary>
        /// Just a basic way to keep track of what the enemy has more
        /// </summary>
        public Dictionary<UnitTypes, int> UnitsFound { get; set; }

        Vector3 buildPosition;
        public Vector3 rootBasePosition;
        private bool isRootBasePositionInitialized;
        public List<Unit> Army { get; set; }
        public List<Unit> Workers { get; set; }
        public AISenses senses;

        public override void Start()
        {
            base.Start();

            _selfRace = info.GetPlayerRace() == Races.MEN ? Races.ELVES : Races.MEN;

            //Init lists
            EnemyUnits = new List<Unit>();
            EnemyBuildings = new List<IGameEntity>();
            OwnBuildings = new List<IGameEntity>();
            modules = new List<AIModule>();
            Army = new List<Unit>();
            //rootBasePosition = new Vector3(706, 80, 765);
            //Army.Add(Info.get.createUnit(_selfRace, UnitTypes.HERO, rootBasePosition,Quaternion.Euler(0,0,0)).GetComponent<Unit>());
            Workers = new List<Unit>();
            Macro = new MacroManager(this);

            //We need to implement som kind of senses for te AI so here they are 
            GameObject sensesContainer = new GameObject("AI Senses");
            sensesContainer.AddComponent<AISenses>();
            senses = sensesContainer.GetComponent<AISenses>();

            Micro = new MicroManager(this);
            modules.Add(new AIModule(Macro.MacroHigh, 30));
            modules.Add(new AIModule(Macro.MacroLow, 5));
            modules.Add(new AIModule(Micro.Micro, 1));
            timers = new float[modules.Count];
            for (int i = 0; i < modules.Count; i++)
                timers[i] = 0;
            //buildPosition = new Vector3(706, 80, 765);
            //rootBasePosition = new Vector3(706, 80, 765);
            isRootBasePositionInitialized = false;

            ActorSelector selector = new ActorSelector()
            {
                registerCondition = gameObject => gameObject.GetComponent<FOWEntity>().IsOwnedByPlayer,
                fireCondition = gameObject => true
            };
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.DISCOVERED, OnEntityFound, selector);
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.HIDDEN, OnEntityLost, selector);

            if (AI_DEBUG_ENABLED)
            {
                aiDebug = AIDebugSystem.CreateComponent(gameObject, this);
            }

        }
        void Update()
        {
            for (int i = 0; i < modules.Count; i++)
            {
                timers[i] += Time.deltaTime;
                if (timers[i] > modules[i].period)
                {
                    modules[i].Callback();
                    timers[i] = 0f;
                }
            }
        }
        void OnEnemyDied(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isUnit)
            {
                EnemyUnits.Remove((Unit)g);
            }
            else if (g.info.isBuilding)
            {
                EnemyBuildings.Remove(g);
            }
        }
        void OnEntityFound(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isUnit)
            {
                if (!EnemyUnits.Contains((Unit)g))
                {
                    g.registerFatalWounds(OnEnemyDied);
                    EnemyUnits.Add((Unit)g);
                }
            }
            else if (g.info.isBuilding)
            {
                if (!EnemyBuildings.Contains(g))
                {
                    g.registerFatalWounds(OnEnemyDied);
                    EnemyBuildings.Add(g);
                }
            }
        }
        void OnEntityLost(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isUnit)
            {
                g.unregisterFatalWounds(OnEnemyDied);
                EnemyUnits.Remove((Unit)g);
            }
            else if (g.info.isBuilding)
            {
                g.unregisterFatalWounds(OnEnemyDied);
                EnemyBuildings.Remove(g);
            }
        }
        public void CreateBuilding(BuildingTypes btype)
        {
            GameObject g = Info.get.createBuilding(_selfRace, btype, buildPosition, Quaternion.Euler(0, 0, 0));
            buildPosition += new Vector3(0, 0, 20);
            Resource build = (Resource)g.GetComponent<IGameEntity>();
            build.register(Resource.Actions.CREATE_UNIT, OnCivilCreated);
            build.register(Resource.Actions.DESTROYED, OnBuildingDestroyed);
        }
        void OnCivilCreated(System.Object obj)
        {
            Unit u = (Unit)obj;
            Workers.Add(u);
            u.register(Unit.Actions.DIED, OnUnitDead);
        }
        void OnBuildingDestroyed(System.Object obj)
        {
            GameObject g = (GameObject)obj;
            Resource res = g.GetComponent<Resource>();
            OwnBuildings.Remove(res);
        }
        void OnUnitDead(System.Object obj)
        {
            GameObject g = (GameObject)obj;
            Unit u = g.GetComponent<Unit>();
            if (Workers.Contains(u))
                Workers.Remove(u);
            if (Army.Contains(u))
                Army.Remove(u);
            Micro.OnUnitDead(u);
        }

        // TODO: Should it be handled with events??
        public override void removeEntity(IGameEntity entity) { }
        public override void addEntity(IGameEntity newEntity)
        {
            if (newEntity.info.isArmy)
            {
                Army.Add((Unit) newEntity);
            }
            else if (newEntity.info.isBuilding)
            {
                buildPosition = newEntity.getTransform().position + new Vector3(0,0,30);
                if (newEntity.info.isBarrack)
                {
                    // TODO rootBasePosition should be initialized elsewhere
                    if (!isRootBasePositionInitialized)
                    {
                        if (((BuildingInfo) newEntity.info).type == BuildingTypes.STRONGHOLD)
                        {
                            rootBasePosition = newEntity.getTransform().position;
                            isRootBasePositionInitialized = !isRootBasePositionInitialized;
                        }
                    }
                    return;
                }
                Resource build = (Resource) newEntity;
                build.register(Resource.Actions.CREATE_UNIT, OnCivilCreated);
                build.register(Resource.Actions.DESTROYED, OnBuildingDestroyed);
            }
        }

        public void addToArmy(List<Unit> units)
        {
            foreach (Unit u in units)
                addToArmy(u);
        }
        public void addToArmy(Unit u)
        {
            Army.Add(u);
            Micro.assignUnit(u);
        }
    }
    struct AIModule
    {
        public AIModule(Action Callback, int period) {
            this.Callback = Callback;
            this.period = period;
        }
        public Action Callback;
        /// <summary>
        /// Time in seconds between each call to Callback
        /// </summary>
        public int period;
    }
    
}
