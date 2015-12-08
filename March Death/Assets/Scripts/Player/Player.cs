using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player : BasePlayer
{
    /// <summary>
    /// Information regarding the current status of the player
    /// </summary>
    private status _currently;
    public status currently { get { return _currently; }}
    public enum status {IDLE, PLACING_BUILDING, SELECTED_UNITS, TERMINATED /*...*/}

    /// <summary>
    /// Information regarding the entities of the player
    /// </summary>
    private List<IGameEntity> _activeEntities = new List<IGameEntity>();
    public List<IGameEntity> activeEntities {
        get { return new List<IGameEntity>(_activeEntities); }
    }


    //the list of player units in the scene
    public ArrayList currentUnits = new ArrayList ();
    
    private EventsNotifier events;

    private bool isGameOverScreenDisplayed = false;

    private CameraController cam;

    private int minFoodTolerance;
    private int minWoodTolerance;
    private int minMetalTolerance;
    private int minGoldTolerance;

    private bool foodDepleted;

    // Use this for initialization
    public override void Start()
    {   
        base.Start();
        _selection = GetComponent<Managers.SelectionManager>();
        //request the race of the player
        _selfRace = info.GetPlayerRace();
        _selection.SetRace(race);
        
        cam = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
        events = GetComponent<EventsNotifier>();

        Battle.PlayerInformation me = info.GetBattle().GetPlayerInformationList()[playerId - 1];
        InstantiateBuildings(me.GetBuildings());
        InstantiateUnits(me.GetUnits());
        SetInitialResources(me.GetResources().Wood, me.GetResources().Food, me.GetResources().Metal, me.GetResources().Gold);
        // gameObject.AddComponent<ResourcesPlacer>();
        
        missionStatus = new MissionStatus(playerId);
        ResourcesPlacer r = ResourcesPlacer.get; // initialization

        // TODO Set this values dynamically
        minFoodTolerance = 100;
        minWoodTolerance = 500;
        minMetalTolerance = 500;
        minGoldTolerance = 500;

        foodDepleted = resources.getAmount(WorldResources.Type.FOOD) <= 0;

        ActorSelector selector = new ActorSelector()
        {
            registerCondition = (g) => !(g.GetComponent<FOWEntity>().IsOwnedByPlayer),
            fireCondition = (g) => true
        };
        Utils.Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.DISCOVERED, OnEntityFound, selector);

    }

    // Update is called once per frame
    void Update()
    {
        if (missionStatus.isGameOver())
        {
            if (!isGameOverScreenDisplayed)
            {
                GameObject gameOverDialog = null;
                if (missionStatus.hasWon(playerId))
                {
                    switch (_selfRace)
                    {
                        case Storage.Races.MEN:
                            gameOverDialog = (GameObject) Resources.Load("GameEndWinHuman");
                            break;
                        case Storage.Races.ELVES:
                            gameOverDialog = (GameObject) Resources.Load("GameEndWinElf");
                            break;
                    }
                }
                else
                {
                    switch (_selfRace)
                    {
                        case Storage.Races.MEN:
                            gameOverDialog = (GameObject) Resources.Load("GameOver-Human");
                            break;
                        case Storage.Races.ELVES:
                            gameOverDialog = (GameObject) Resources.Load("GameOver-Elf");
                            break;
                    }
                }
                Instantiate(gameOverDialog);
                isGameOverScreenDisplayed = true;
            }
            _currently = status.TERMINATED;
        }
    }

    public override void OnDestroy()
    {
        _currently = status.TERMINATED;
        Utils.Subscriber<FOWEntity.Actions, FOWEntity>.get.unregisterFromAll(FOWEntity.Actions.DISCOVERED, OnEntityFound);

        base.OnDestroy();
    }

    public override void removeEntity(IGameEntity entity)
    {
        _activeEntities.Remove(entity);
        unregisterEntityEvents(entity);
    }

    /// <summary>
    /// Add a IGameEntity to the list
    /// Player has a list with all the entities associated to him
    /// </summary>
    /// <param name="newEntity"></param>
    public override void addEntity(IGameEntity newEntity)
    {
        _activeEntities.Add(newEntity);
        registerEntityEvents(newEntity);
        Debug.Log(_activeEntities.Count + " entities");
    }

	public void FillPlayerUnits(GameObject unit) 
	{
		currentUnits.Add (unit);
	}
    
    /// <summary>
    /// Returns the count of the current associated entities
    /// </summary>
    /// <returns></returns>
    public int currentEntitiesCount()
    {
        return _activeEntities.Count;
    }

    /// <summary>
    /// Returns whether is in the current status or not
    /// </summary>
    /// <param name="check"></param>
    /// <returns></returns>
    public bool isCurrently(status check)
    {
        return _currently == check;

    }

    /// <summary>
    /// Establishes the new status of the player
    /// </summary>
    /// <param name="newStatus"></param>
    public void setCurrently(status newStatus)
    {
        _currently = newStatus;
    }

    private void displayResourceInfo(WorldResources.Type resourceType, int tolerance)
    {
        int amount;
        amount = Mathf.FloorToInt(resources.getAmount(resourceType));
        if (amount <= tolerance)
        {
            if (amount > 0)
                events.DisplayResourceIsLow(resourceType);
            else
                events.DisplayResourceDepleted(resourceType);
        }
    }

    private void onUnitEats(System.Object obj)
    {
        // TODO Take into account goods? Storage.Goods goods = (Storage.Goods) obj;
        if (!foodDepleted)
        {
            displayResourceInfo(WorldResources.Type.FOOD, minFoodTolerance);
            foodDepleted = resources.getAmount(WorldResources.Type.FOOD) <= 0;
        }
    }

    private void OnUnitCreated(System.Object obj)
    {
        displayResourceInfo(WorldResources.Type.FOOD, minFoodTolerance);
        displayResourceInfo(WorldResources.Type.METAL, minMetalTolerance);
        displayResourceInfo(WorldResources.Type.WOOD, minWoodTolerance);
        displayResourceInfo(WorldResources.Type.GOLD, minGoldTolerance);
        events.DisplayUnitCreated(obj);
    }
    
    private void signalMissionUpdate(System.Object obj)
    {
        IGameEntity entity = ((GameObject) obj).GetComponent<IGameEntity>();
        switch (entity.info.entityType)
        {
            case Storage.EntityType.BUILDING:
                missionStatus.OnBuildingDestroyed(entity.info.getType<Storage.BuildingTypes>());
                break;
            case Storage.EntityType.UNIT:
                missionStatus.OnUnitKilled(entity.info.getType<Storage.UnitTypes>());
                break;
        }
    }

    protected override void AddBuilding(IGameEntity entity)
    {
        Storage.BuildingInfo bi;
        addEntity(entity);
        bi = (Storage.BuildingInfo) entity.info;
        if (bi.type == Storage.BuildingTypes.STRONGHOLD)
        {
            cam.lookGameObject(entity.getGameObject());
        }
    }

    protected override void AddUnit(IGameEntity entity)
    {
        addEntity(entity);
    }

    /// <summary>
    /// Registers the events that display a message to the user.
    /// </summary>
    /// <param name="entity">Game entity that triggers the event.</param>
    private void registerEntityEvents(IGameEntity entity)
    {
        if (entity.info.isBuilding)
        {
            if (entity.info.isBarrack)
            {
                Barrack barrack = (Barrack) entity;
                barrack.register(Barrack.Actions.DAMAGED, events.DisplayUnderAttack);
                barrack.register(Barrack.Actions.DESTROYED, events.DisplayBuildingDestroyed);
                barrack.register(Barrack.Actions.CREATE_UNIT, OnUnitCreated);
                barrack.register(Barrack.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
            }
            else 
            {
                Resource resourcesBuilding = (Resource) entity;
                resourcesBuilding.register(Resource.Actions.DAMAGED, events.DisplayUnderAttack);
                resourcesBuilding.register(Resource.Actions.DESTROYED, events.DisplayBuildingDestroyed);
                resourcesBuilding.register(Resource.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
                resourcesBuilding.register(Resource.Actions.CREATE_UNIT, OnUnitCreated);
            }
        }
        else if (entity.info.isUnit)
        {
            Unit unit = (Unit) entity;
            unit.register(Unit.Actions.DAMAGED, events.DisplayUnderAttack);
            unit.register(Unit.Actions.DIED, events.DisplayUnitDead);
            unit.register(Unit.Actions.TARGET_TERMINATED, signalMissionUpdate);
            unit.register(Unit.Actions.EAT, onUnitEats);
        }
    }

    private void unregisterEntityEvents(IGameEntity entity)
    {
        if (entity.info.isBarrack)
        {
            Barrack barrack = (Barrack) entity;
            barrack.unregister(Barrack.Actions.DAMAGED, events.DisplayUnderAttack);
            barrack.unregister(Barrack.Actions.DESTROYED, events.DisplayBuildingDestroyed);
            barrack.unregister(Barrack.Actions.CREATE_UNIT, OnUnitCreated);
            barrack.unregister(Barrack.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
        }
        else if (entity.info.isResource)
        {
            Resource resourcesBuilding = (Resource) entity;
            resourcesBuilding.unregister(Resource.Actions.DAMAGED, events.DisplayUnderAttack);
            resourcesBuilding.unregister(Resource.Actions.DESTROYED, events.DisplayBuildingDestroyed);
            resourcesBuilding.unregister(Resource.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
            resourcesBuilding.unregister(Resource.Actions.CREATE_UNIT, OnUnitCreated);
        }
        else if (entity.info.isUnit)
        {
            Unit unit = (Unit) entity;
            unit.unregister(Unit.Actions.DIED, events.DisplayUnitDead);
            unit.unregister(Unit.Actions.DAMAGED, events.DisplayUnderAttack);
            unit.unregister(Unit.Actions.TARGET_TERMINATED, signalMissionUpdate);
            unit.unregister(Unit.Actions.EAT, onUnitEats);
        }
    }

    private void OnEntityFound(System.Object obj)
    {
        GameObject go = (GameObject) obj;
        IGameEntity entity = go.GetComponent<IGameEntity>();
        if (entity == null) return;    // HACK Sometimes, it is a LightHouse-Revealer, i.e. there is no game entity (NullReferenceException)
        if (entity.info.isUnit)
        {
            events.DisplayEnemySpotted(go);
        }
    }
}
