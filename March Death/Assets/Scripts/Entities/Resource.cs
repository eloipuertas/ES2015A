using System;
using System.Reflection;
using UnityEngine;
using Storage;



public class Resource : GameEntity<Resource.Actions>
{
    public enum Actions { DAMAGED, DESTROYED };

    // Constructor
    public Resource() { }

    /// <summary>
    /// Edit this on the Prefab to set Resources of certain races/types
    /// </summary>
    public Races race = Races.MEN;
    public ResourceTypes type = ResourceTypes.FARM;


    /// <summary>
    /// Time elapsed to next update
    /// </summary>
    public float updateInterval { get; set; }

    /// <summary>
    ///  Next update time
    /// </summary>
    //
    private float nextUpdate;

    /// <summary>
    // Resource could store limited amount of material when not in use
    // or when production is higher than collection
    /// </summary>
    //
    private int storeSize;
    /// <summary>
    /// amount of materials produced and not collected yet.
    /// Maximun allowed is storeSize
    /// </summary>
    private int stored;

    /// <summary>
    /// Material amount produced every time interval.
    /// </summary>
    public int productionRate { get; set; }

    /// <summary>
    /// sum of capacity of units collecting this resource
    /// the more units the more maxCollectionRate.
    /// real collectionRate could be lower due to
    /// store limit.
    /// </summary>
    public int collectionRate { get; set; }


    /// <summary>
    ///  units collecting this resource
    /// </summary>
    public int harvestUnits;
    //public int harvestUnits { get; set; }



    /// <summary>
    /// max harvesting units allowed
    /// </summary>
    public int maxUnits { get; set; }

    /// <summary>
    /// material amount collected when update succes.
    /// </summary>
    private int collectedAmount;

    /// <summary>
    /// When a wound is received, this is called
    /// </summary>
    protected override void onReceiveDamage()
    {
        fire(Actions.DAMAGED);
    }

    /// <summary>
    /// When wounds reach its maximum, thus unit dies, this is called
    /// </summary>
    protected override void onFatalWounds()
    {
        fire(Actions.DESTROYED);
    }

    /// <summary>
    /// when collider interact with other gameobject method checks if
    /// it is collecting unit and if unit has the rigth type for collecting
    ///  resource.Then update number of collectors attached and production.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {

        // space enough to hold new collectingUnit
        if (harvestUnits < _info.resourceAttributes.maxUnits)
        {
            IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();

            // check collection unit and right type
            if (entity.info.isCivil)
            {
                Unit unit = (Unit)entity;
                UnitInfo info = (UnitInfo)entity.info;

                //Increase units collecting resource, calculate max capacity.
                if (match(info.type, type))
                {
                    harvestUnits++;
                    collectionRate += info.attributes.capacity;
                }
            }
        }


    }
    void OnTriggerStay(Collider other)
    {
        ;
    }

    void OnTriggerExit(Collider other)
    {

        // get entity
        IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();

        if (harvestUnits < _info.resourceAttributes.maxUnits)
        {
            if (entity.info.isCivil)
            {
                UnitInfo info = (UnitInfo)entity.info;

                //Decrease units collecting resource, calculate max capacity.
                if (match(info.type, type))
                {
                    harvestUnits--;
                    collectionRate -= info.attributes.capacity;
                }
            }
        }
    }

    /// <summary>
    /// check if collecting unit type matchs rigth resource type
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="type"></param>
    /// <returns>
    /// true if resource and unit type match,
    /// false otherwise
    /// </returns>
    bool match(UnitTypes unitType, ResourceTypes type)
    {
        if (type.Equals(ResourceTypes.FARM))
        {
            return unitType.Equals(UnitTypes.FARMER);
        }
        if (type.Equals(ResourceTypes.MINE))
        {
            return unitType.Equals(UnitTypes.MINER);
        }
        if (type.Equals(ResourceTypes.SAWMILL))
        {
            return unitType.Equals(UnitTypes.LUMBERJACK);
        }
        return false;

    }
    void collect()
    {
        if (collectionRate > stored)
        {
            // collect all stored resources
            collectedAmount = stored;
            stored = 0;
        }
        else
        {
            // collection capacity lower than stored materials
            collectedAmount = collectionRate;
            stored -= collectedAmount;
        }
        addResource(collectedAmount);
        return;
    }

    void produce()
    {
        int remainingSpace = _info.resourceAttributes.storeSize - stored;
        if (_info.resourceAttributes.productionRate > remainingSpace)
        {
            stored = _info.resourceAttributes.storeSize;
        }
        else
        {
            stored += _info.resourceAttributes.productionRate;
        }
        return;
    }

    void addResource(int amount)
    {
        if (type.Equals(ResourceTypes.FARM))
        {
            //TODO
            //add amount to player food
        }
        if (type.Equals(ResourceTypes.MINE))
        {
            //TODO
            //add amount to player metal
        }
        if (type.Equals(ResourceTypes.SAWMILL))
        {
            //TODO
            //add amount to player wood
        }
        return;
    }



    /// <summary>
    /// Object initialization
    /// </summary>
    override public void Start()
    {
        type = ResourceTypes.FARM;
        race = Races.MEN;
        nextUpdate = 0;
        stored = 0;
        collectionRate = 0;
        harvestUnits = 0;

        _status = EntityStatus.IDLE;
        _info = Info.get.of(race, type);

        // Call GameEntity start
        base.Start();
    }


    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (Time.time > nextUpdate)
        {
            nextUpdate = Time.time + _info.resourceAttributes.updateInterval;
            // when updated, collector units load materials from store.
            // after they finish loading materials production cycle succes.
            // new produced materials can be stored but not collected until
            // next update.
            collect();
            produce();

        }

    }
}
