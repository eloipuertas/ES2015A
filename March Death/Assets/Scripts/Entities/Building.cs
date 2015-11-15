using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;
using Utils;

/// <summary>
/// Building base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public abstract class Building<T> : GameEntity<T> where T : struct, IConvertible
{
    public Building() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public BuildingTypes type = BuildingTypes.STRONGHOLD;
    public override E getType<E>() { return (E)Convert.ChangeType(type, typeof(E)); }

    /// Precach some actions
    public T DAMAGED { get; set; }
    public T DESTROYED { get; set; }

	private float totalBuildTime = 0;
	private int woundsBuildControl = 0;


    /// <summary>
    /// When a wound is received, this is called
    /// </summary>
    protected override void onReceiveDamage()
    {
        fire(DAMAGED);
    }

    /// <summary>
    /// When wounds reach its maximum, thus unit dies, this is called
    /// </summary>
    protected override void onFatalWounds()
    {
        fire(DESTROYED);
    }

    public override IKeyGetter registerFatalWounds(Action<System.Object> func)
    {
        return register(DESTROYED, func);
    }

    public override IKeyGetter unregisterFatalWounds(Action<System.Object> func)
    {
        return unregister(DESTROYED, func);
    }

    /// <summary>
    /// When destroyed, it's called
    /// </summary>
    public override void OnDestroy() 
	{
		try {
			ConstructionGrid grid = GameObject.Find("GameController").GetComponent<ConstructionGrid>();
			Vector3 disc_pos = grid.discretizeMapCoords(gameObject.transform.position);
			grid.liberatePosition(disc_pos);
		} catch(Exception e) {
			Debug.LogWarning("Exception while trying to liberate position of a building: " + e.ToString());
		}
		base.OnDestroy();
	}
	
	/// <summary>
    /// Object initialization
    /// </summary>
    public override void Awake()
    {
        DAMAGED = (T)Enum.Parse(typeof(T), "DAMAGED", true);
        DESTROYED = (T)Enum.Parse(typeof(T), "DESTROYED", true);

        // Call GameEntity start
        base.Awake();
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Start()
    {
        // Setup base
        base.Start();

		activateFOWEntity();
		_woundsReceived = info.buildingAttributes.wounds;
		woundsBuildControl = info.buildingAttributes.wounds;

		//return (info.buildingAttributes.wounds - _woundsReceived) * 100f / info.buildingAttributes.wounds;

        // Set the status
        setStatus(EntityStatus.BUILDING_PHASE_1);
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    public override void Update()
    {
        base.Update();

		// Control the building phases, as well as wounds while being built.
		// TODO: Figure out a better condition... 
		if (status == EntityStatus.BUILDING_PHASE_3 || status == EntityStatus.BUILDING_PHASE_2 || status == EntityStatus.BUILDING_PHASE_1) {
			totalBuildTime += Time.deltaTime;
			float buildingPercentage = (totalBuildTime) / info.buildingAttributes.timeToBuild;
			int woundsBuilt = (int) ((1 - buildingPercentage) * info.buildingAttributes.wounds);
			int diffWounds =  woundsBuildControl - woundsBuilt;

			if (diffWounds > 0) {
				// We are substracting wounds instead of a new value because the building might be under attack while is being built.
				_woundsReceived -= diffWounds;
				woundsBuildControl = woundsBuilt;
			}

			// TODO: What if we have more than 3 phases... maybe we should add the number of phases in the JSON, instead of harcoding it...
			if (buildingPercentage > 0.33 && buildingPercentage <=0.66) {
				setStatus(EntityStatus.BUILDING_PHASE_2);			
			} else if (buildingPercentage > 0.66 && buildingPercentage < 1) {
				setStatus(EntityStatus.BUILDING_PHASE_3);				
			} else if (buildingPercentage >= 1) {
				setStatus(EntityStatus.IDLE);				
			}
		}
    }

    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
    }
}
