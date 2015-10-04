using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;



public class Resource : Utils.Actor<Resource.Actions>, IGameEntity
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
    public int harvestUnits { get; set; }



    /// <summary>
    /// max harvesting units allowed
    /// </summary>
    public int maxUnits { get; set; }

    /// <summary>
    /// material amount collected when update succes.
    /// </summary>
    private int collectedAmount;

    /// <summary>
    /// List of ability objects of this resource
    /// </summary>
    private List<IResourceAbility> _abilities;

    /// <summary>
    /// Contains all static information of the Resource.
    /// That means: max health, damage, defense, etc.
    /// </summary>
    private ResourceInfo _info;
    private Storage.ResourceAttributes _attributes;
    public EntityInfo info
    {
        get
        {
            return _info;
        }
    }

    /// <summary>
    /// Returns current status of the Resource
    /// </summary>
    private EntityStatus _status;
    public EntityStatus status
    {
        get
        {
            return _status;
        }
    }

    /// <summary>
    /// Returns the number of wounds a Resource received
    /// </summary>
    private float _woundsReceived;
    public float wounds
    {
        get
        {
            return _woundsReceived;
        }
    }


    /// <summary>
    /// Returns percentual value of health (100% meaning all life)
    /// </summary>
    public float healthPercentage
    {
        get
        {
            return (_attributes.wounds - _woundsReceived) * 100f / _attributes.wounds;
        }
    }


    /// <summary>
    /// Returns percentual value of damage (100% meaning 0% life)
    /// </summary>
    public float damagePercentage
    {
        get
        {
            return 100f - healthPercentage;
        }
    }

    /// <summary>
    /// Returns true in case an attack will land on this resource
    /// </summary>
    /// <param name="from">Unit which attacked</param>
    /// <param name="isRanged">Set to true in case the attack is range, false if melee</param>
    /// <returns>True if it hits, false otherwise</returns>
    private bool willAttackLand(Unit from, bool isRanged = false)
    {
        int dice = Utils.D6.get.rollSpecial();

        if (isRanged)
        {
            // TODO: Specil units (ie gigants) and distance!
            return dice > 1 && (_attributes.projectileAbility + dice >= 7);
        }

        return HitTables.meleeHit[((UnitAttributes)from.info.attributes).weaponAbility, _attributes.weaponAbility] <= dice;
    }

    /// <summary>
    /// Retuns true if an attack will cause wounds to this resource
    /// </summary>
    /// <param name="from">Attacker</param>
    /// <returns>True if causes wounds, false otherwise</returns>
    private bool willAttackCauseWounds(Unit from)
    {
        int dice = Utils.D6.get.rollOnce();

        return HitTables.wounds[((UnitAttributes)from.info.attributes).strength, _attributes.resistance] <= dice;
    }

    /// <summary>
    /// Automatically calculates if an attack will hit, and in case it
    /// does it updates the current state.
    /// </summary>
    /// <param name="from">Attacker</param>
    /// <param name="isRanged">True if the attack is ranged, false if melee</param>
    public void receiveAttack(Unit from, bool isRanged)
    {
        // Do not attack dead targets
        if (_status == EntityStatus.DEAD)
        {
            throw new InvalidOperationException("Can not receive damage while not alive");
        }

        // If it hits and produces damage, update wounds
        if (willAttackLand(from, isRanged) && willAttackCauseWounds(from))
        {
            _woundsReceived += 1;
            fire(Actions.DAMAGED);
        }

        // Check if we are dead
        if (_woundsReceived == _attributes.wounds)
        {
            _status = EntityStatus.DEAD;
            //_target = null;

            fire(Actions.DESTROYED);
        }
    }

    /// <summary>
    /// Iterates all abilities on the resource
    /// </summary>
    private void setupAbilities()
    {
        _abilities = new List<IResourceAbility>();

        foreach (ResourceAbility ability in _info.actions)
        {
            // Try to get class with this name
            string abilityName = ability.name.Replace(" ", "");

            try
            {
                var constructor = Type.GetType(abilityName).
                    GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(UnitAbility), typeof(GameObject) }, null);
                if (constructor == null)
                {
                    // Invalid constructor, use GenericAbility
                    _abilities.Add(new GenericResourceAbility(ability));
                }
                else
                {
                    // Class found, use that!
                    _abilities.Add((IResourceAbility)constructor.Invoke(new object[2] { ability, gameObject }));
                }
            }
            catch (Exception /*e*/)
            {
                // No such class, use the GenericAbility class
                _abilities.Add(new GenericResourceAbility(ability));
            }
        }
    }


    /// <summary>
    /// Returns an action given a name
    /// </summary>
    /// <param name="name">Name of the action</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when no action with the given name is found
    /// </exception>
    /// <returns>Always returns a valid IResourceAbility (IAction)</returns>
    public IAction getAction(string name)
    {
        foreach (IResourceAbility ability in _abilities)
        {
            if (ability.info.name.Equals(name))
            {
                return ability;
            }
        }

        throw new ArgumentException("Invalid action " + name + "requested");
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
        if (harvestUnits < maxUnits)
        {
            IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();

            // check collection unit and right type
            if (entity.info.isCivil)
            {
                Unit unit = (Unit)entity;
                UnitInfo info = entity.info.toUnitInfo;
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

        if (harvestUnits < maxUnits)
        {
            if (entity.info.isCivil)
            {
                UnitInfo info = entity.info.toUnitInfo;
                //Decrease units collecting resource, calculate max capacity.
                if (match(info.type, type))
                {
                    harvestUnits--;
                    collectionRate -= info.attributes.capacity;
                }
            }
        }

        Destroy(other.gameObject);


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
        int remainingSpace = storeSize - stored;
        if (productionRate > remainingSpace)
        {
            stored = storeSize;
        }
        else
        {
            stored += productionRate;
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
    void Start()
    {

        type = ResourceTypes.FARM;
        race = Races.MEN;
        nextUpdate = 0;
        stored = 0;
        
        harvestUnits = 0;
        updateInterval = 1;
        productionRate = 20;
        
        collectionRate = 0;


        _status = EntityStatus.IDLE;   
        _info = Info.get.of(race, type);
        
        _attributes = (Storage.ResourceAttributes)_info.attributes;
        setupAbilities();

        Debug.Log(_info);
        Debug.Log(_info.attributes);

        Debug.Log(info);
        Debug.Log(info.attributes);
        Debug.Log(_attributes);

        Debug.Log(_info.attributes.wounds);
        Debug.Log(((Storage.ResourceAttributes)info.attributes).wounds);
        Debug.Log(_attributes.weaponAbility);

    }





	// Update is called once per frame
	void Update () {

        if (Time.time > nextUpdate)
        {
            nextUpdate = Time.time + updateInterval;
            // when updated, collector units load materials from store.
            // after they finish loading materials production cycle succes.
            // new produced materials can be stored but not collected until
            // next update.
            collect();
            produce();

        }

    }
    /// <summary>    
    /// Returns NULL as this cannot be converted to Unit
	/// </summary>
    /// <returns>Object casted to Unit</returns>
    public Unit toUnit() { return null; }

    /// <summary>
    /// Casts this IGameEntity to Unity (pointless if already building)
    /// </summary>
    /// <returns>Always null</returns>
    public Building toBuilding() { return null; }

    /// <summary>
    /// Casts this IGameEntity to Resource (pointless if already resource)
    /// </summary>
    /// <returns>Always null</returns>
    public Resource toResource() { return this; }

}
