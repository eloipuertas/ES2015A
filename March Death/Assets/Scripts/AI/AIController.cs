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
        public List<Resource> OwnResources { get; set; }
        public List<Barrack> OwnBarracks { get; set; }
        public List<IGameEntity> EnemyBuildings { get; set; }
        public List<Unit> EnemyUnits { get; set; }
        /// <summary>
        /// Just a basic way to keep track of what the enemy has more
        /// </summary>
        public Dictionary<UnitTypes, int> UnitsFound { get; set; }

        Vector3 buildPosition;
        public Vector3 rootBasePosition;
        public List<Unit> Army { get; set; }
        public List<Unit> Workers { get; set; }
        public AISenses senses;

        // HACK To signal MicroManager that the game has ended when the AI hero is killed
        public bool FinishPlaying { get { return missionStatus.isGameOver(); } }

        public override void Start()
        {
            base.Start();
            DifficultyLvl = 2; //TODO remove when this gets assigned from the menu
            _selfRace = info.GetPlayerRace() == Races.MEN ? Races.ELVES : Races.MEN;

            //Init lists
            EnemyUnits = new List<Unit>();
            EnemyBuildings = new List<IGameEntity>();
            OwnResources = new List<Resource>();
            OwnBarracks = new List<Barrack>();
            modules = new List<AIModule>();
            Army = new List<Unit>();
            Workers = new List<Unit>();

            Battle.PlayerInformation me = info.GetBattle().GetPlayerInformationList()[playerId - 1];
            SetInitialResources(me.GetResources().Wood, me.GetResources().Food, me.GetResources().Metal, me.GetResources().Gold);
            Battle.PlayableEntity.EntityPosition pos = me.GetBuildings()[0].position;
            rootBasePosition = new Vector3(pos.X, 80, pos.Y);
            buildPosition = rootBasePosition;
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


            InstantiateBuildings(me.GetBuildings());
            InstantiateUnits(me.GetUnits());

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

            missionStatus = new MissionStatus(playerId);

        }
        
        void Update()
        {
            if (!missionStatus.isGameOver())
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
        }
        void OnEnemyDied(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isUnit)
            {
                EnemyUnits.Remove((Unit)g);
                missionStatus.OnUnitKilled(((Unit) g).type);
            }
            else if (g.info.isBuilding)
            {
                EnemyBuildings.Remove(g);
                missionStatus.OnBuildingDestroyed(g.getType<Storage.BuildingTypes>());
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

        /// <summary>
        /// Need to unify this method with players one.
        /// </summary>
        /// <param name="race"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool isAffordable(Storage.Races race, Storage.BuildingTypes type)
        {
            Storage.BuildingInfo i = Storage.Info.get.of(race, type);

            return (resources.getAmount(WorldResources.Type.FOOD) >= i.resources.food &&
                    resources.getAmount(WorldResources.Type.WOOD) >= i.resources.wood &&
                    resources.getAmount(WorldResources.Type.METAL) >= i.resources.metal);
        }

        /// <summary>
        /// Used to pay something
        /// </summary>
        /// <param name="entity"></param>
        public void checkout(IGameEntity entity)
        {
            resources.SubstractAmount(WorldResources.Type.FOOD, entity.info.resources.food);
            resources.SubstractAmount(WorldResources.Type.WOOD, entity.info.resources.wood);
            resources.SubstractAmount(WorldResources.Type.METAL, entity.info.resources.metal);
        }

        public void CreateBuilding(BuildingTypes btype)
        {
            GameObject g = Info.get.createBuilding(_selfRace, btype, buildPosition, Quaternion.Euler(0, 0, 0));
            buildPosition += new Vector3(0, 0, 20);
            IGameEntity entity = g.GetComponent<IGameEntity>();
            OnBuildingCreated(entity);
            checkout(entity);

        }

        public void CreateBuilding(BuildingTypes btype, Vector3 position, Quaternion rotation)
        {
            GameObject g = Info.get.createBuilding(_selfRace, btype, position, rotation);
            IGameEntity entity = g.GetComponent<IGameEntity>();
            OnBuildingCreated(entity);
            if(!AIArchitect.TESTING) checkout(entity);
        }

        void OnBuildingCreated(IGameEntity entity)
        {
            if (entity.info.isResource)
            {
                OwnResources.Add((Resource)entity);
            }
            else
            {
                OwnBarracks.Add((Barrack)entity);
            }
        }

        void OnUnitCreated(Unit u)
        {
            if (u.info.isCivil)
            {
                //Workers.Add(u);
                //The line above is correct, but we still don't have enemy units so let's just put everything into the army and wipe the floor with the player
                addToArmy(u);
            }
            else
            {
                addToArmy(u);
            }
        }
        
        public override void removeEntity(IGameEntity entity) {
            if (entity.info.isBuilding)
            {
                if(entity.info.isResource)
                {
                    OwnResources.Remove((Resource)entity);
                }
                else
                {
                    OwnBarracks.Remove((Barrack)entity);
                }
            }
            else
            {
                Unit u = (Unit)entity;
                Micro.OnUnitDead(u);
                if (entity.info.isArmy)
                {
                    if (Army.Contains(u))
                        Army.Remove(u);
                }
                else
                {
                    if (Workers.Contains(u))
                        Workers.Remove(u);
                }
            }
        }
        public override void addEntity(IGameEntity newEntity)
        {
            OnUnitCreated((Unit)newEntity);
        }

        //For some reasons AddUnit and AddBuilding only get created at the start
        protected override void AddUnit(IGameEntity entity)
        {
            OnUnitCreated((Unit)entity);
        }
        protected override void AddBuilding(IGameEntity entity)
        {
            buildPosition = entity.getTransform().position + new Vector3(0,0,30);
            OnBuildingCreated(entity);
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
