using System;
using System.Reflection;
using UnityEngine;
using Storage;


public class Resource : Building<Resource.Actions>
{
    public enum Actions { DAMAGED, DESTROYED, COLLECTION, CREATE_UNIT };
    // Constructor
    public Resource() { }

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
    ///  units collecting this resource
    /// </summary> 
    private int harvestUnits { get; set; }
 
    /// <summary>
    /// material amount send to player (collected) when update succes.
    /// </summary>
    private float _collectedAmount;

    /// <summary>
    /// civilians pay budgets and generate gold every update interval
    /// </summary>
    private float _goldAmount;

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
        return unitType == UnitTypes.CIVIL;
    }

    /// <summary>
    /// civilians units collect resources each production cicle.
    /// the sum of units capacity is the total amount of materials they can 
    /// take from the store and send to player. Gold is collected via 
    /// gold taxes and sent to the user, it is not stored
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
        
        //TODO gold consum and production unit attributes needed
        //_goldAmount = harvestUnits * (info.attributes.goldProduction - info.attributes.goldConpsumption);
        sendResource(_collectedAmount, 0);
        return;
    }

    /// <summary>
    /// after civilians sends last batch produced they are able to take the 
    /// new production and store it for the next production cicle
    /// </summary>
    private void produce()
    {
        float remainingSpace = info.resourceAttributes.storeSize - _stored;
        
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
    /// new goods produced are sent to player.
    /// Method triger an event sending object goods with amount of materials 
    /// transferred. gold production is sent too.
    /// </summary>
    /// <param name="amount"></param>
    private void sendResource(float amount, float gold)
    {
        if ((amount + gold) > 0.0)
        {
            Goods goods = new Goods();
            goods.amount = amount;
            goods.gold = gold;

            if (type.Equals(BuildingTypes.FARM))
            {
                goods.type = Goods.GoodsType.FOOD;
            }
            else if(type.Equals(BuildingTypes.MINE))
            {
                goods.type = Goods.GoodsType.METAL;
            }
            else{ goods.type = Goods.GoodsType.WOOD; }
            fire(Actions.COLLECTION, goods);
        }         
    }
    /// <summary>
    /// Method create civilian unit.
    /// If capacity limit of building is not reached unit is positioned inside 
    /// building limits otherwise unit is positioned outside, 
    /// just at desired meeting Point.
    /// civilian sex is randomly selected(last parameter of createUnit method).
    /// </summary>
    /// <returns>civilian Gameobect</returns>
    public void createCivilian()
    {
        // TODO set desired rotation, now unit rotation equals building rotation!!
        // TODO  ---create gameobject meetingPointInside and meetingPointOutside
        // attached to resource building design---
        
        //---unComment next two lines when meeting point objects are created---
        //GameObject meetingPointInside = this.GetComponent(meetingPointInside);
        //GameObject meetingPointOutside = this.GetComponent(meetingPointOutside);

        
        if (harvestUnits < info.resourceAttributes.maxUnits)
        {
            // TODO get inside meeting point and calculate position
            //unitPosition = this.GetComponent(meetingPointInside).transform.position;

            // Units distributed in rows of 5 elements
            _xDisplacement = harvestUnits % 6;
            _yDisplacement = harvestUnits / 6;
            _unitPosition.Set(transform.position.x + _xDisplacement, transform.position.y + _yDisplacement, transform.position.z);
            GameObject civil = Info.get.createUnit(race, UnitTypes.CIVIL, _unitPosition, _unitRotation, -1);
            harvestUnits++;

           _collectionRate += Info.get.of(race, UnitTypes.CIVIL).attributes.capacity; 

            fire(Actions.CREATE_UNIT, civil);
        }
        else
        {
            // TODO get outside meeting point and calculate position
     
            _unitPosition.Set(transform.position.x + 10 , transform.position.y, transform.position.z);
            GameObject civil = Info.get.createUnit(race, UnitTypes.CIVIL, _unitPosition, _unitRotation, -1);
            fire(Actions.CREATE_UNIT, civil);
            // TODO method to modify unit coordinates to avoid unit overlap
        }
  
    }

    /// <summary>
    /// Recruit a Explorer from building. you need to do this to take away worker
   ///  from building. production decrease when you remove workers
   /// </summary>
    public void recruitExplorer(GameObject worker)
    {
        if (harvestUnits > 0)
        {
            _collectionRate -= Info.get.of(race, UnitTypes.CIVIL).attributes.capacity;  
            harvestUnits--;
        } 
        // TODO: Some alert message if you try to remove unit when no unit at building
    }

    /// <summary>
    /// Recruit a worker. you can use a explorer as a worker. beware of building maxUnits.
    /// </summary>
    public void recruitWorker(GameObject explorer)
    {
        if (harvestUnits < info.resourceAttributes.maxUnits)
        {
            _collectionRate -= Info.get.of(race, UnitTypes.CIVIL).attributes.capacity;
            harvestUnits++;
        }
        // TODO: Some alert message if you try to recruit worker and building has no vacancy
    }





    /// <summary>
    /// when collider interact with other gameobject method checks if 
    /// gameobject is a civilian unit. Civilians units are recruited as workers
    /// while limit of workers are not reached.  
    /// </summary>
    /// <param name="other">collider gameobject interacting with our own collider</param>
    void OnTriggerEnter(Collider other)
    {
        
        // space enough to hold new civil
        
        if (harvestUnits < info.resourceAttributes.maxUnits)
        {
            IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();

            // check if unit is civil
            if (entity.info.isCivil)
            {
                recruitWorker(other.gameObject);
            }
           
        }

    }
    void OnTriggerStay(Collider other)
    {
        ;
    }
    /// <summary>
    /// when collider interaction with other gameobject ends method checks if
    /// gameobject is civilian unit. Civilians units are recruited as explorers
    /// and fired as workers.
    /// </summary>
    /// <param name="other">collider gameobject interacting with our own collider</param>
    void OnTriggerExit(Collider other)
    {

        // get entity
        IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();

        if (harvestUnits < info.resourceAttributes.maxUnits)
        {
            if (entity.info.isCivil)
            {
                recruitExplorer(other.gameObject);
            }
        }
    }


    /// <summary>
    /// Object initialization
    /// </summary>
    override public void Start()
    {       
        _nextUpdate = 0;
        _stored = 0;
        _collectionRate = 0;
        harvestUnits = 0; 
        _status = EntityStatus.IDLE;
        _info = Info.get.of(race, type);
        // new resource building has 1 civilian when created
        createCivilian();
        // Call Building start
        base.Start();
    }


    // Update is called once per frame
    // when updated, collecting units load materials from store and send it to
    // player.After they finish sending materials, production cycle succes.
    // new produced materials can be stored but not collected until
    // next update.
    override public void Update()
    {
        base.Update();
        if (Time.time > _nextUpdate)
        {
            _nextUpdate = Time.time + info.resourceAttributes.updateInterval;  
            collect();
            produce();            
        }
    }
}
