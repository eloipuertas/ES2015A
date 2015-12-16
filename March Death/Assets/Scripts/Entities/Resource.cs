using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Storage;
using UnityEngine.Assertions;


public class Resource : Building<Resource.Actions>
{

    public enum Actions { CREATED, DAMAGED, DESTROYED, BUILDING_FINISHED, COLLECTION, CREATE_UNIT, DEL_STATS, HEALTH_UPDATED, ADDED_QUEUE, NEW_HARVEST, EXTERMINATED, NEW_EXPLORER };

    /// <summary>
    /// civilian creation waste some time. When units are being created status
    /// changes to RUN. Process can only be started while IDLE status.
    /// </summary>
    public enum createCivilStatus { IDLE, RUN, DISABLED };

    /// <summary>
    ///  Sounds manager
    /// </summary>
    Managers.SoundsManager sounds;

    // Constructor
    public Resource() { }

    private IGameEntity _entity;

    private createCivilStatus _createStatus;

    /// <summary>
    /// status of the create civilian option.
    /// RUN when civilian unit is being created
    /// IDLE when option is availabe.
    /// DISABLE when option is disabled.
    /// </summary>
    ///
    public createCivilStatus buttonCivilStatus
    {
        get
        {
            return _createStatus;
        }
    }

    /// <summary>
    /// Controls time elapsed since civil creation process was started
    /// </summary>
    private float endConstructionTime;

    /// <summary>
    ///  Next update time
    /// </summary>
    private float _nextUpdate;

    /// <summary>
    // Resource could store limited amount of material when not in use
    // or when production is higher than collection
    /// </summary>
    private float _stored;

    /// <summary>
    /// sum of capacity of units collecting this resource
    /// the more units the more collectionRate.
    /// real collectionRate could be lower due to
    /// store limit.
    /// </summary>
    private float _collectionRate { get; set; }

    /// <summary>
    ///  units currently working and collecting this resource
    /// </summary>
    ///
    public int harvestUnits { get; private set; }

    /// <summary>
    /// info of civilian units
    /// </summary>
    private UnitInfo civilInfo;

    /// <summary>
    /// list of civilian units working at this resource
    /// </summary>
    List<Unit> workersList = new List<Unit>();

    /// <summary>
    /// max harvest units this resource can have
    /// </summary>
    public int maxHarvestUnits { get; private set; }


    /// <summary>
    /// HUD, get current civilian units working here
    /// </summary>
    public int HUD_currentWorkers
    {
        get
        {
            return harvestUnits;
        }
    }

    /// <summary>
    /// HUD, max production rate for this resource building and level.
    /// </summary>
    public int HUD_productionRate
    {
        get
        {
            return info.resourceAttributes.productionRate;
        }
    }

    /// <summary>
    /// HUD, max production rate for this resource building and level.
    /// </summary>
    public int HUD_currentProductionRate
    {
        get
        {
            return civilInfo.attributes.capacity * harvestUnits;
        }
    }

    /// <summary>
    /// HUD, max storing capacity for this level and resource building type
    /// </summary>
    public int HUD_storeSize
    {
        get
        {
            return info.resourceAttributes.storeSize;
        }
    }

    /// <summary>
    /// number of units created by this building. dead or alive
    /// </summary>
    public int totalUnits { get; private set; }

    /// <summary>
    /// material amount send to player (collected) when update succes.
    /// </summary>
    private float _collectedAmount;

    /// <summary>
    /// this building can create units.
    /// unitPosition are the x,y,z map coordinates of new civilian
    /// </summary>
    private Vector3 _unitPosition;

    /// <summary>
    /// this building can create units.
    /// unitRotation is the rotation of new civilian
    /// </summary>
    private Quaternion _unitRotation;

    /// <summary>
    /// when you create a civilian some displacement is needed to avoid units
    /// overlap. this is the x-axis displacement
    /// </summary>
    private int _xDisplacement;

    /// <summary>
    /// when you create a civilian some displace is needed to avoid units
    /// overlap. this is the y-axis displacement
    /// </summary>
    private int _yDisplacement;

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
    /// current player
    /// </summary>
    private Player player;

    /// <summary>
    /// check if starter unit was created. We need to wait until resource is built
    /// </summary>
    public bool hasDefaultUnit { get; private set; }


    private readonly object syncLock = new object();
    bool hasCreatedCivil = false;
    

    List<GameObject> pendingProducers = new List<GameObject>();
    List<GameObject> pendingWanderers = new List<GameObject>();


    /// <summary>
    /// check if collecting unit type matchs rigth resource type
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="type"></param>
    /// <returns>
    /// true if resource and unit type match,
    /// false otherwise
    /// </returns>
    private bool match(UnitTypes unitType, BuildingTypes type)
    {
        return unitType == UnitTypes.CIVIL;
    }

    /// <summary>
    /// civilians units collect resources each production cicle.
    /// the sum of units capacity is the total amount of materials they can
    /// take from the store and send to player.
    /// </summary>
    private void collect()
    {

        if (_collectionRate > _stored)
        {
            // collect all stored resources
            _collectedAmount = _stored;
            _stored = 0;
        }
        else
        {
            // collection capacity lower than stored materials. some materials
            //remain at store until new collection cycle.
            _collectedAmount = _collectionRate;
            _stored -= _collectedAmount;
        }
        sendResource(_collectedAmount);
        return;
    }

    /// <summary>
    /// after civilians sends last batch produced they are able to take the
    /// new production and store it for the next production cycle
    /// </summary>
    private void produce()
    {
        float remainingSpace = info.resourceAttributes.storeSize - _stored;

        // Production rate bigger than remaining store space means we will
        // lose part or whole production!!

        if (info.resourceAttributes.productionRate >= remainingSpace)
        {
            _stored = info.resourceAttributes.storeSize;
        }
        else
        {
            _stored += info.resourceAttributes.productionRate;
        }
        return;
    }

    /// <summary>
    /// New goods produced are sent to player.
    /// Method triger an event sending object goods with amount of materials
    /// transferred. gold production is sent too.
    /// </summary>
    /// <param name="amount">materials amount produced</param>
    ///
    /// TODO: now we are using two diferent ways to increase player resources
    /// 1- Create classe goods and send it to player using event.
    /// 2- Direct use of addAmount method.
    ///
    /// we must change this behaviour, only one way will be the right one.

    private void sendResource(float amount)
    {

        if (amount > 0.0)
        {
            CollectableGood collectable = new CollectableGood();
            collectable.entity = this;
            collectable.goods = new Goods();
            collectable.goods.amount = amount;

            // TODO:
            // BUG: Null reference when we try to add material amount to player.

            if (type.Equals(BuildingTypes.FARM))
            {
                collectable.goods.type = WorldResources.Type.FOOD;
            }
            else if (type.Equals(BuildingTypes.MINE))
            {
                collectable.goods.type = WorldResources.Type.METAL;
            }
            else
            {
                collectable.goods.type = WorldResources.Type.WOOD;
            }
            fire(Actions.COLLECTION, collectable);
        }
    }

    /// <summary>
    /// Method overrides base to check if civilian unit will be a worker or a explorer.
    /// If this unit must be a worker it is vanished and placed at center of building.
    /// </summary>
    /// <param name="type"></param>
    protected override void createUnit(UnitTypes type)
    {
        if (harvestUnits < maxHarvestUnits)
        {
            _unitPosition.Set(_center.x, _center.y, _center.z);
            GameObject gob = Info.get.createUnit(race, type, _unitPosition, _unitRotation, -1);
            Unit civil = gob.GetComponent<Unit>();
            civil.vanish();
            civil.setStatus(EntityStatus.WORKING);
            BasePlayer.getOwner(this).addEntity(civil);
            fire(Actions.CREATE_UNIT, civil);

            totalUnits++;
            harvestUnits++;
            workersList.Add(civil);
            setStatus(EntityStatus.WORKING);

            _collectionRate += Info.get.of(race, UnitTypes.CIVIL).attributes.capacity;

            fire(Actions.NEW_HARVEST, (IGameEntity)this);
        }
        else
        {
            base.createUnit(type);
        }

        _createStatus = createCivilStatus.IDLE;
    }
    /// <summary>
    /// Method create civilian unit.
    /// If capacity limit of building is not reached unit is positioned inside
    /// building limits otherwise unit is positioned outside,
    /// just at desired deployment Point.
    /// civilian sex is randomly selected(last parameter of createUnit method).
    /// </summary>
    /// <returns>civilian GameObject</returns>
    public void newCivilian()
    {
        base.addUnitQueue(UnitTypes.CIVIL);
    }


    /// <summary>
    /// Recruit a Explorer from building. you need to do this to take away worker
    ///  from building. production decrease when you remove workers
    /// </summary>
    public Unit recruitExplorer()
    {
        if (harvestUnits > 0)
        {
            Unit worker;
            worker = workersList.PopAt(0);
            _collectionRate -= worker.info.attributes.capacity;
            harvestUnits--;

            worker.transform.position = getDeploymentPoint();
            worker.bringBack();
            worker.moveTo(findMeetingPoint());


            worker.setStatus(EntityStatus.IDLE);

            if (harvestUnits == 0)
            {
                setStatus(EntityStatus.IDLE);
            }

            fire(Actions.NEW_EXPLORER, (IGameEntity)this);
            return worker;
        }
        else
        {
            Debug.Log("Can't recruite explorer because no workers");
            return null;
        }
        // TODO: Some alert message or sound for player if try to remove unit when no unit at building
    }

    /// <summary>
    /// Recruit a worker. you can use a explorer as a worker. beware of building maxUnits.
    /// </summary>
    private void recruitWorker(Unit explorer)
    {
        if (harvestUnits < maxHarvestUnits)
        {
            _collectionRate += explorer.info.attributes.capacity;
            harvestUnits++;

            explorer.setStatus(EntityStatus.WORKING);

            workersList.Add(explorer);
            if (harvestUnits == 1)
            {
                setStatus(EntityStatus.WORKING);
            }

            fire(Actions.NEW_HARVEST, (IGameEntity)this);
        }
        else
        {
            Debug.Log(" You are trying to recruit worker but building capacity is full");
        }

    }

    /// <summary>
    /// If civilian unit points to building and is close enough (inside trapRange)
    /// Unit is vanished and teleported to building. Civilian units at building
    /// are recruited as workers.
    /// </summary>
    /// <param name="entity"></param>
    public void trapUnit(IGameEntity entity)
    {
        Debug.Log("Unit trapped");
        // Unit must be civil and player owned
        Assert.IsTrue(entity.info.isCivil);
        Assert.IsTrue(entity.info.race == info.race);

        // space enough to hold new civil
        if (harvestUnits < maxHarvestUnits)
        {
            Unit unit = (Unit)entity;
            unit.vanish();
            sounds.onExplorerTrapped();
            unit.transform.position = this.transform.position;
            recruitWorker((Unit)unit);
        }
        else
        {
            sounds.onFullHouse();
        }
    }

    /// <summary>
    /// When building is destroyed civilian workers turns into explorers
    /// </summary>
    public override void OnDestroy()
    {
        fire(Actions.EXTERMINATED, _entity);
        base.OnDestroy();
    }

    private WorldResources.Type ResourceFromBuilding(BuildingTypes type)
    {
        switch (type)
        {
            case BuildingTypes.FARM:
                return WorldResources.Type.FOOD;
            case BuildingTypes.MINE:
                return WorldResources.Type.METAL;
            case BuildingTypes.SAWMILL:
                return WorldResources.Type.WOOD;
            default:
                throw new Exception("That resource type does not exist!");
        }
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    override public void Awake()
    {
        _nextUpdate = 0;
        _stored = 0;
        _collectionRate = 0;
        harvestUnits = 0;
        _xDisplacement = 0;
        _yDisplacement = 0;
        _info = Info.get.of(race, type);
        totalUnits = 0;
        _unitRotation = transform.rotation;
        hasDefaultUnit = false;
        civilInfo = Info.get.of(this.race, UnitTypes.CIVIL);
        _entity = this.GetComponent<IGameEntity>();
        sounds = GameObject.Find("GameController").GetComponent<Managers.SoundsManager>();

        // Call Building start
        base.Awake();
    }

    override public void Start()
    {
        // Setup base
        base.Start();
        this.GetComponent<Rigidbody>().isKinematic = false;

        maxHarvestUnits = info.resourceAttributes.maxUnits;
    }


    // Update is called once per frame
    // when updated, collecting units load materials from store and send it to
    // player.After they finish sending materials, production cycle succes.
    // new produced materials can be stored but not collected until
    // next update.
    override public void Update()
    {
        base.Update();

        switch (status)
        {
            case EntityStatus.IDLE:

                if (!hasDefaultUnit)
                {
                    hasDefaultUnit = true;
                }
                break;

            case EntityStatus.WORKING:
                if (Time.time > _nextUpdate)
                {
                    _nextUpdate = Time.time + info.resourceAttributes.updateInterval;
                    collect();
                    produce();
                }
                break;
        }
    }

    /// <summary>
    /// When built, it's called
    /// </summary>
    protected override void onBuilt()
    {
        base.onBuilt();
        newCivilian();
    }
}

/// <summary>
/// Class to pop element from list. Weird thing , sure.
/// </summary>
///
static class ListExtension
{
    public static T PopAt<T>(this List<T> list, int index)
    {
        T r = list[index];
        list.RemoveAt(index);
        return r;
    }
}
