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

    // i order to mantain InformationController working
	//public ArrayList SelectedObjects = new ArrayList();
    public ArrayList SelectedObjects { get { return _selection.ToArrayList(); } }

    private EventsNotifier events;

    private bool isGameOverScreenDisplayed = false;

    private CameraController cam;

    // Use this for initialization
    public override void Start()
    {   
        Debug.Log("Player Start");base.Start();
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
        gameObject.AddComponent<ResourcesPlacer>();
        missionStatus = new MissionStatus(playerId);

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

    void OnDestroy()
    {
        _currently = status.TERMINATED;
    }

    public override void removeEntity(IGameEntity entity)
    {
        _activeEntities.Remove(entity);
        unregisterEventDisplayMessages(entity);
    }

    /// <summary>
    /// Add a IGameEntity to the list
    /// Player has a list with all the entities associated to him
    /// </summary>
    /// <param name="newEntity"></param>
    public override void addEntity(IGameEntity newEntity)
    {
        _activeEntities.Add(newEntity);
        if (newEntity != null) registerEventDisplayMessage(newEntity);
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

    public ArrayList getSelectedObjects()
    {
        return (ArrayList) SelectedObjects.Clone();
    }

    // <summary>
    // Getter for the resources of the player.
    // </summary>
    //public Managers.ResourcesManager resources {get { return _resources; } }

    private void performUnitDied(System.Object obj)
    {
        IGameEntity e = ((GameObject) obj).GetComponent<IGameEntity>();
        missionStatus.OnUnitKilled(((Unit) e).type);
    }

    private void performBuildingDestroyed(System.Object obj)
    {
        IGameEntity e = ((GameObject) obj).GetComponent<IGameEntity>();
        if (e.info.isBarrack)
        {
            missionStatus.OnBuildingDestroyed(((Barrack) e).type);
        }
        else if (e.info.isResource)
        {
            missionStatus.OnBuildingDestroyed(((Resource) e).type);
        }
    }

    /// <summary>
    /// Registers events for when an enemy game entity is killed or destroyed.
    /// </summary>
    /// <param name="entity">Enemy game entity.</param>
    public void registerGameEntityActions(IGameEntity entity)
    {
        if (entity.info.isUnit)
        {
            entity.registerFatalWounds(performUnitDied);
        }
        else if (entity.info.isBuilding)
        {
            entity.registerFatalWounds(performBuildingDestroyed);
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
    private void registerEventDisplayMessage(IGameEntity entity)
    {
        if (entity.info.isBuilding)
        {
            if (entity.info.isBarrack)
            {
                Barrack barrack = (Barrack) entity;
                barrack.register(Barrack.Actions.DAMAGED, events.DisplayUnderAttack);
                barrack.register(Barrack.Actions.DESTROYED, events.DisplayBuildingDestroyed);
                barrack.register(Barrack.Actions.CREATE_UNIT, events.DisplayUnitCreated);
                barrack.register(Barrack.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
            }
            else 
            {
                Resource resourcesBuilding = (Resource) entity;
                resourcesBuilding.register(Resource.Actions.DAMAGED, events.DisplayUnderAttack);
                resourcesBuilding.register(Resource.Actions.DESTROYED, events.DisplayBuildingDestroyed);
                resourcesBuilding.register(Resource.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
                resourcesBuilding.register(Resource.Actions.CREATE_UNIT, events.DisplayUnitCreated);
            }
        }
        else if (entity.info.isUnit)
        {
            Unit unit = (Unit) entity;
            unit.register(Unit.Actions.DAMAGED, events.DisplayUnderAttack);
            unit.register(Unit.Actions.DIED, events.DisplayUnitDead);
        }
    }

    private void unregisterEventDisplayMessages(IGameEntity entity)
    {
        if (entity.info.isBarrack)
        {
            Barrack barrack = (Barrack) entity;
            barrack.unregister(Barrack.Actions.DAMAGED, events.DisplayUnderAttack);
            barrack.unregister(Barrack.Actions.DESTROYED, events.DisplayBuildingDestroyed);
            barrack.unregister(Barrack.Actions.CREATE_UNIT, events.DisplayUnitCreated);
            barrack.unregister(Barrack.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
        }
        else if (entity.info.isResource)
        {
            Resource resourcesBuilding = (Resource) entity;
            resourcesBuilding.unregister(Resource.Actions.DAMAGED, events.DisplayUnderAttack);
            resourcesBuilding.unregister(Resource.Actions.DESTROYED, events.DisplayBuildingDestroyed);
            resourcesBuilding.unregister(Barrack.Actions.BUILDING_FINISHED, events.DisplayBuildingCreated);
            resourcesBuilding.unregister(Resource.Actions.CREATE_UNIT, events.DisplayUnitCreated);
        }
        else if (entity.info.isUnit)
        {
            Unit unit = (Unit) entity;
            unit.unregister(Unit.Actions.DIED, events.DisplayUnitDead);
            unit.unregister(Unit.Actions.DAMAGED, events.DisplayUnderAttack);
        }
    }
}
