using UnityEngine;
using System.Collections;
using Assets.Scripts.AI;

public class BasePlayer : Utils.SingletonMono<BasePlayer> {

    protected BasePlayer() {}

    /// <summary>
    /// The race of the player
    /// </summary>
    protected Storage.Races _selfRace;
    public Storage.Races race { get { return _selfRace; } }

    /// <summary>
    /// The resources manager
    /// </summary>
    protected Managers.ResourcesManager _resources;
    public Managers.IResourcesManager resources { get { return _resources; } }

    /// <summary>
    /// The buildings manager
    /// </summary>
    protected Managers.BuildingsManager _buildings;
    public Managers.BuildingsManager buildings { get { return _buildings;  } }


    /// <summary>
    /// The units manager
    /// </summary>
    protected Managers.UnitsManager _units;
    public Managers.UnitsManager units { get { return _units;  } }

    static GameInformation info = null;
    static BasePlayer player = null;
    static BasePlayer ia = null;


    public virtual void Start ()
    {
        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");

        info = gameController.GetComponent<GameInformation>();
        player = gameController.GetComponent<Player>();
        ia = gameController.GetComponent<AIController>();
    }

    static BasePlayer getOwner(IGameEntity entity)
    {
        if (entity.info.race == info.GetPlayerRace())
        {
            return player;
        }

        return ia;
    }

    void Update () {}
}
