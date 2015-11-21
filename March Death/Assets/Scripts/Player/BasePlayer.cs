using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.AI;

public abstract class BasePlayer : Utils.SingletonMono<BasePlayer> {

    protected BasePlayer() { }

    /// <summary>
    /// The race of the player
    /// </summary>
    protected Storage.Races _selfRace;
    public Storage.Races race { get { return _selfRace; } }

    /// <summary>
    /// The resources manager
    /// </summary>
    protected Managers.ResourcesManager _resources = new Managers.ResourcesManager();
    public Managers.ResourcesManager resources { get { return _resources; } }

    /// <summary>
    /// The buildings manager
    /// </summary>
    protected static Managers.BuildingsManager _buildings;
    public Managers.BuildingsManager buildings { get { return _buildings; } }



    /// <summary>
    /// The selection Manager
    /// </summary>
    protected Managers.SelectionManager _selection;
    public Managers.SelectionManager selection { get { return _selection; } }
     



    protected static GameInformation _info = null;
    protected static BasePlayer _player = null;
    protected static BasePlayer _ia = null;

    public static GameInformation info { get { return _info; } }
    public static Player player { get { return (Player)_player; } }
    public static AIController ia { get { return (AIController)_ia; } }

    protected int playerId = 0;
    public int PlayerID { set { playerId = value; } }

    protected MissionStatus missionStatus;

    Terrain terrain;

    public virtual void Start ()
    {
        GameObject gameInformationObject = GameObject.Find("GameInformationObject");
        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        _info = gameInformationObject.GetComponent<GameInformation>();        
    }

    public static void Setup()
    {
        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");

        _buildings = gameController.GetComponent<Main_Game>().BuildingsMgr;        
        _player = gameController.GetComponent<Player>();
        _ia = gameController.GetComponent<AIController>();
    }

    public abstract void removeEntity(IGameEntity entity);
    public abstract void addEntity(IGameEntity newEntity);

    public static BasePlayer getOwner(IGameEntity entity)
    {
        if (entity.info.race == info.GetPlayerRace())
        {
            return player;
        }

        return ia;
    }

    void Update () {}

    public void SetInitialResources(uint wood, uint food, uint metal, uint gold)
    {
        // TODO Consider adding a maximum capacity
        _resources.InitDeposit(new WorldResources.Resource(WorldResources.Type.FOOD, food));
        _resources.InitDeposit(new WorldResources.Resource(WorldResources.Type.WOOD, wood));
        _resources.InitDeposit(new WorldResources.Resource(WorldResources.Type.METAL, metal));
        _resources.InitDeposit(new WorldResources.Resource(WorldResources.Type.GOLD, gold));
    }

    protected abstract void AddBuilding(IGameEntity entity);
    protected abstract void AddUnit(IGameEntity entity);

    protected void InstantiateBuildings(List<Battle.PlayableEntity> buildings)
    {
        GameObject created;
        Vector3 position;
        foreach (Battle.PlayableEntity building in buildings)
        {
            position = new Vector3();
            position.x = building.position.X;
            position.z = building.position.Y;
            // HACK Without the addition, Construction Grid detects the terrain as it not being flat
            position.y = 1 + terrain.SampleHeight(position);
            created = _buildings.createBuilding(position, Quaternion.Euler(0,0,0),
                                        building.type.building,
                                        _selfRace);
            AddBuilding(created.GetComponent<IGameEntity>());
        }
    }

    protected void InstantiateUnits(List<Battle.PlayableEntity> units)
    {
        GameObject created;
        Vector3 position;
        foreach (Battle.PlayableEntity unit in units)
        {
            position = new Vector3();
            position.x = unit.position.X;
            position.z = unit.position.Y;
            position.y = terrain.SampleHeight(position);
            created = Storage.Info.get.createUnit(_selfRace, unit.type.unit,
                                          position, Quaternion.Euler(0,0,0));
            AddUnit(created.GetComponent<IGameEntity>());
        }
    }
}
