using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;
using Utils;
using System.Collections;


/// <summary>
/// Building base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public abstract class Building<T> : GameEntity<T>, IBuilding where T : struct, IConvertible
{
    public Building() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public BuildingTypes type = BuildingTypes.STRONGHOLD;
    public override E getType<E>() { return (E)Convert.ChangeType(type, typeof(E)); }
    
    /// <summary>
    /// Used to store values returned from getObjectsNearPosition method.
    /// </summary>
    private GameObject[] nearObjects;
    private EntityStatus _defaultStatus = EntityStatus.BUILDING_PHASE_1;
    public override EntityStatus DefaultStatus
    {
        get
        {
            return _defaultStatus;
        }
        set
        {
            _defaultStatus = value;
        }
    }

    /// Precach some actions
    public T DAMAGED { get; set; }
    public T DESTROYED { get; set; }
    public T CREATE_UNIT { get; set; }
    public T BUILDING_FINISHED { get; set; }
    //public T HEALTH_UPDATED { get; set; }
    public T ADDED_QUEUE { get; set; }
    
    private float _totalBuildTime = 0;
    private float _creationTimer = 0;
    private int _woundsBuildControl = 0;
    private UnitInfo _infoUnitToCreate;
    private bool _creatingUnit = false;

    /// <summary>
    /// coordinates where new civilians are deployed outside building
    ///  reached.
    /// </summary> 
    private Vector3 _deploymentPoint;

    /// <summary>
    /// coordinates where civilian units travel after been deployed at deployment point.
    /// </summary>
    private Vector3 _meetingPoint;

    private int _totalUnits = 0;

    // This queue will store the units that the building is creating.
    private Queue<UnitTypes> _creationQueue = new Queue<UnitTypes>();

    /// <summary>
    /// When a wound is received, this is called
    /// </summary>
    protected override void onReceiveDamage()
    {
		base.onReceiveDamage ();
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
        GameObject gridGO = GameObject.Find("GameController");

        if (gridGO != null) {
			ConstructionGrid grid = gridGO.GetComponent<ConstructionGrid>();
			Vector3 disc_pos = grid.discretizeMapCoords(gameObject.transform.position);
			grid.liberatePosition(disc_pos);
		} 

		base.OnDestroy();
	}


    /// <summary>
    /// When built, it's called
    /// </summary>
    protected virtual void onBuilt()
    {
        fire (BUILDING_FINISHED);
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Awake()
    {
        DAMAGED = (T)Enum.Parse(typeof(T), "DAMAGED", true);
        DESTROYED = (T)Enum.Parse(typeof(T), "DESTROYED", true);
        CREATE_UNIT = (T)Enum.Parse(typeof(T), "CREATE_UNIT", true);
        BUILDING_FINISHED = (T) Enum.Parse(typeof(T), "BUILDING_FINISHED", true);
        HEALTH_UPDATED = (T)Enum.Parse(typeof(T), "HEALTH_UPDATED", true);
        ADDED_QUEUE = (T)Enum.Parse(typeof(T), "ADDED_QUEUE", true);
        
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


        _meetingPoint = getDefaultMeetingPoint();
        activateFOWEntity();

        if (DefaultStatus == EntityStatus.BUILDING_PHASE_1)
        {
            _woundsReceived = info.buildingAttributes.wounds;
            _woundsBuildControl = info.buildingAttributes.wounds;
        }

        //return (info.buildingAttributes.wounds - _woundsReceived) * 100f / info.buildingAttributes.wounds;
        // Set the status
        setStatus(DefaultStatus);
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    public override void Update()
    {
        base.Update();

        // Control the building phases, as well as wounds while being built.
        // TODO: Figure out a better condition...
        if (status == EntityStatus.BUILDING_PHASE_3 || status == EntityStatus.BUILDING_PHASE_2 || status == EntityStatus.BUILDING_PHASE_1)
        {
            _totalBuildTime += Time.deltaTime;
            float buildingPercentage = (_totalBuildTime) / info.buildingAttributes.creationTime;
            int woundsBuilt = (int)((1 - buildingPercentage) * info.buildingAttributes.wounds);
            int diffWounds = _woundsBuildControl - woundsBuilt;

            if (diffWounds > 0)
            {
                // We are substracting wounds instead of a new value because the building might be under attack while is being built.
                _woundsReceived -= diffWounds;
                _woundsBuildControl = woundsBuilt;
                fire(HEALTH_UPDATED);
            }

			// TODO: What if we have more than 3 phases... maybe we should add the number of phases in the JSON, instead of harcoding it...
			if (buildingPercentage > 0.33 && buildingPercentage <=0.66) {
				setStatus(EntityStatus.BUILDING_PHASE_2);
			} else if (buildingPercentage > 0.66 && buildingPercentage < 1) {
				setStatus(EntityStatus.BUILDING_PHASE_3);
			} else if (buildingPercentage >= 1) {
				setStatus(EntityStatus.IDLE);
                onBuilt();
            }
		}else if (_creatingUnit)
        {
            _creationTimer += Time.deltaTime;

            if (_creationTimer >= _infoUnitToCreate.unitAttributes.creationTime)
            {
				_creationQueue.Dequeue();
                createUnit(_infoUnitToCreate.type);
                _creatingUnit = false;
            }
        }
        else if (_creationQueue.Count > 0)
        {
            _infoUnitToCreate = Info.get.of(info.race, _creationQueue.Peek());
            _creationTimer = 0;
            _creatingUnit = true;
        }
    }

    /// <summary>
    ///  x, y, z coordinates of our building
    /// </summary>
    private Vector3 _center
    {
        get
        {
            return transform.position;
        }
    }

    /// <summary>
    /// Deployment point, units are deployed here if position is available.
    /// </summary>
    /// <returns></returns>
    public Vector3 getDeploymentPoint()
    {
        Vector3 position = new Vector3();
        position = ConstructionGrid.instance.getFreePositionAbleToConstructNearPoint(_center);
       
        return _center + ((position - _center)/1.5F);

    }
    /// <summary>
    /// Default meeting point. If meeting point is not set unit travel here from deployment point
    /// </summary>
    /// <returns></returns>
    public Vector3 getDefaultMeetingPoint()
    {
        Vector3 position = new Vector3();
        position = ConstructionGrid.instance.getFreePositionAbleToConstructNearPoint(_center);
       
        return position;

    }

    /// <summary>
    /// Default Meeting point where new units walk from deployment point
    /// </summary>
    /// <returns>position of meetingpoint</returns>
    public void setMeetingPoint(Vector3 position)
    {
        _meetingPoint = position;
    }

    /// <summary>
    /// Current Meeting point where new units walk from deployment point
    /// </summary>
    /// <returns>position of meetingpoint</returns>
    public Vector3 getMeetingPoint()
    {
        return _meetingPoint;
    }

    /// <summary>
    /// check if meeting point is still available
    /// </summary>
    /// <returns>position of meetingpoint</returns>
    public Vector3 findMeetingPoint()
    {

        Vector3 position = new Vector3();
        GameObject[] objects = Helpers.getObjectsNearPosition(getMeetingPoint(), 1);
        foreach (GameObject g in objects)
        {
            if (g.GetComponent<IBuilding>() != null)
            {
                position = getDefaultMeetingPoint();
                return position;
            }
        }
        return getMeetingPoint();

    }

    protected virtual void createUnit(UnitTypes type)
    {       
        GameObject gob = Info.get.createUnit(race, type, getDeploymentPoint(), transform.rotation, -1);
        Unit new_unit = gob.GetComponent<Unit>(); 
        BasePlayer.getOwner(this).addEntity(new_unit);
        fire(CREATE_UNIT, new_unit);
        _totalUnits++;
    
        new_unit.moveTo(findMeetingPoint());

    }

    public bool addUnitQueue(UnitTypes type)
    {
        if (_creationQueue.Count < info.buildingAttributes.creationQueueCapacity)
        {
            _creationQueue.Enqueue(type);
            fire(ADDED_QUEUE, type);
            return true;
        }
        else
        {
            Debug.LogWarning("Creation queue reached its limit");
            return false;
        }
    }

    /// <summary>
    /// Returns creation percentage
    /// </summary>
    public float getcreationUnitPercentage()
    {
       
        if (_infoUnitToCreate!= null )
        {
            return (_creationTimer * 100f) / _infoUnitToCreate.unitAttributes.creationTime;
        }
        return 0F;
       
    }

    /// <summary>
    /// Returns the number of units that are in the creationQueue
    /// </summary>
    public int getNumberElements()
    {
        return _creationQueue.Count;
    }


    /// <summary>
    /// Pops a unit from the queue, preventing it from being created
    /// </summary>
    public void cancelUnitQueue()
    {
        if (_creationQueue.Count > 0)
        {
            IGameEntity entity = gameObject.GetComponent<IGameEntity>();
            UnitInfo unitInfo = Info.get.of(info.race, (UnitTypes)_creationQueue.Dequeue());
			_creatingUnit = false;

            Player.getOwner(entity).resources.AddAmount(WorldResources.Type.WOOD, unitInfo.resources.wood);
            Player.getOwner(entity).resources.AddAmount(WorldResources.Type.METAL, unitInfo.resources.metal);
            Player.getOwner(entity).resources.AddAmount(WorldResources.Type.FOOD, unitInfo.resources.food);
        }
    }

    /// <summary>
    /// Returns the creation queue.
    /// </summary>
    public Queue<UnitTypes> getCreationQueue()
    {
        return _creationQueue;
    }

    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
    }
}
