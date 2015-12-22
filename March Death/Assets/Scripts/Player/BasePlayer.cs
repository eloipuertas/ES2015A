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
    /// <summary>
    /// Sets the player ID. Make sure to use a number greater than zero.
    /// </summary>
    /// <value>The player I.</value>
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


    public static BasePlayer getOwner(Storage.Races race)
    {
        if (race == info.GetPlayerRace())
        {
            return player;
        }

        return ia;
    }

    public static BasePlayer getOwner(IGameEntity entity)
    {
        return getOwner(entity.info.race);
    }

    public static bool isOfPlayer(IGameEntity entity)
    {
        return getOwner(entity) == player;
    }

    void Update () {}

    public void SetInitialResources(uint wood, uint food, uint metal, uint gold)
    {
        // TODO Consider adding a maximum capacity
        ResourcesPlacer.get(this).InitializeResources(wood, food, metal, gold);
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
            position.y = terrain.SampleHeight(position);
            created = _buildings.createBuilding(position, Quaternion.Euler(0,0,0),
                                        building.type.building,
                                        _selfRace, false, 1.0f);

            IGameEntity entity = created.GetComponent<IGameEntity>();
            if (building.hasStatus)
            {
                entity.DefaultStatus = building.status;
                entity.setStatus(building.status);
            }

            AddBuilding(entity);
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
