using UnityEngine;
using System.Collections;

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



    
    void Start () {}
	
	
	void Update () {}
}
