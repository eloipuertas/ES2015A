using System;
using System.Reflection;
using UnityEngine;
using Storage;


public class Resource : Building
{
    public new enum Actions { DAMAGED, DESTROYED, COLLECTION_START, COLLECTION_STOP, CREATE_UNIT };
    // Constructor
    public Resource() { }

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
    /// check if collecting unit type matchs rigth resource type
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="type"></param>
    /// <returns>
    /// true if resource and unit type match,
    /// false otherwise
    /// </returns>
    bool match(UnitTypes unitType, BuildingTypes type)
    {
        if (type.Equals(BuildingTypes.FARM))
        {
            return unitType.Equals(UnitTypes.FARMER);
        }
        if (type.Equals(BuildingTypes.MINE))
        {
            return unitType.Equals(UnitTypes.MINER);
        }
        if (type.Equals(BuildingTypes.SAWMILL))
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
        if (type.Equals(BuildingTypes.FARM))
        {
            //TODO
            //add amount to player food
        }
        if (type.Equals(BuildingTypes.MINE))
        {
            //TODO
            //add amount to player metal
        }
        if (type.Equals(BuildingTypes.SAWMILL))
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
        // Call actor start
        base.Start();

        type = BuildingTypes.FARM;
        race = Races.MEN;
        nextUpdate = 0;
        stored = 0;
        collectionRate = 0;
        harvestUnits = 0;

        this.status = EntityStatus.IDLE;

        _info = Info.get.of(race, type);
        _attributes = (Storage.BuildingAttributes)info.attributes;
        setupAbilities();
    }


    // Update is called once per frame
    void Update()
    {

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
