using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class AbstractPlayer
{
    /// <summary>
    /// The resources manager
    /// </summary>
    protected Managers.ResourcesManager _resourcesManager;
    public Managers.IResourcesManager resourcesManager { get { return _resourcesManager; } }
    
    /// <summary>
    /// The buildings manager
    /// </summary>
    protected Managers.BuildingsManager _buildingsManager;
    public Managers.BuildingsManager buildingsManager { get { return _buildingsManager;  } }
    
    
    /// <summary>
    /// The units manager
    /// </summary>
    protected Managers.UnitsManager _unitsManager;
    public Managers.UnitsManager unitsManager { get { return _unitsManager;  } }

    /// <summary>
    /// The race of the player
    /// </summary>
    protected Storage.Races _selfRace;
    public Storage.Races race { get { return _selfRace; } }

    /// <summary>
    /// List with this player's buildings
    /// </summary>
    protected List<IGameEntity> Buildings { get; set; }

    /// <summary>
    /// List with this player's army
    /// </summary>
    protected List<Unit> Army{ get; set; }

    /// <summary>
    /// List with this player's workers (civilians)
    /// </summary>
    /// <value>The workers.</value>
    protected List<Unit> Workers { get; set; }
}

