using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Storage;

public class Archery : Building<Archery.Actions>
{
    public enum Actions { DAMAGED, DESTROYED, CREATE_UNIT };
    /// <summary>
    /// Create Archer HUD button status
    /// IDLE: New Archers units construction queue is empty and available
    /// RUN: Construccion queue is running. free space for new demands still
    ///      available
    /// FULL: Construction queue is full.
    /// DISABLED: HUD option not available
    /// </summary>
    public enum createArcherStatus { IDLE, RUN, FULL, DISABLED };

    // Constructor
    public Archery() { }

    private IGameEntity _entity;
    private createArcherStatus _createStatus;

    /// <summary>
    /// Create Archer HUD button status
    /// IDLE: New Archers units construction queue is empty and available
    /// RUN: Construccion queue is running. free space for new demands still
    ///      available
    /// FULL: Construction queue is full.
    /// DISABLED: HUD option not available
    /// </summary>
    public createArcherStatus buttonNewArcherStatus
    {
        get
        {
            return _createStatus;
        }
    }

    /// <summary>
    /// new Archer Unit creation spends some time. MakingNewArcher is true if unit 
    /// construction is in process, false otherwise.
    /// </summary>
    private bool _makingNewArcher = false;

    /// <summary>
    /// New Archer is being made.
    /// </summary>
    public bool HUD_newArcherBeingMade
    {
        get
        {
            return _makingNewArcher;
        }
    }
    /// <summary>
    /// time period spent making new archery unit
    /// </summary>
    private float _constructionTime;

    /// <summary>
    /// Controls time elapsed since Archer unit creation process was started
    /// </summary>
    private float endConstructionTime;

    /// <summary>
    ///  Next update time
    /// </summary>
    private float _nextUpdate;

    /// <summary>
    /// info of archery units
    /// </summary>
    private UnitInfo unitInfo;

    /// <summary>
    /// number of units created by this building.
    /// </summary>
    public int totalUnits { get; private set; }

    /// <summary>
    /// this building can create units.
    /// unitPosition are the x,y,z map coordinates of new unit
    /// </summary>
    private Vector3 _unitPosition;

    /// <summary>
    /// this building can create units.
    /// unitRotation is the rotation of new unit
    /// </summary>
    private Quaternion _unitRotation;

    /// <summary>
    /// when you create new unit some displacement is needed to avoid units 
    /// overlap. this is the x-axis displacement
    /// </summary>
    private int _xDisplacement;

    /// <summary>
    /// when you create new unit some displacement is needed to avoid units 
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
    /// True if building construction process is finished
    /// False otherwise
    /// </summary>
    private bool _isReady;

    /// <summary>
    /// Maximum size of the construction unit queue
    /// </summary>
    private int queueSize;

    /// <summary>
    /// Queue of units in creation process
    /// </summary>
    Queue<Action> archersQueue;
    

    public void createArcher()
    {
        
        if (archersQueue.Count == 0)
        {
            _createStatus = createArcherStatus.RUN;
            setStatus(EntityStatus.WORKING);
        }
        if (archersQueue.Count < queueSize)
        {  
            archersQueue.Enqueue(() => { newArcher(); });
            if (archersQueue.Count == queueSize)
            {
                _createStatus = createArcherStatus.FULL;
            }
        }
        else
        {
            Debug.Log("Queue is full");
        }           
    }
    private void newArcher()
    {
        totalUnits++;
        _xDisplacement = totalUnits % 5;
        _yDisplacement = totalUnits / 5;
        _unitPosition.Set(_center.x + 10 + _xDisplacement, _center.y, _center.z + 10 + _yDisplacement);
        GameObject gob = Info.get.createUnit(race, UnitTypes.ARCHER, _unitPosition, _unitRotation, -1);
        Unit archer = gob.GetComponent<Unit>();
        BasePlayer.getOwner(this).addEntity(archer);
        fire(Actions.CREATE_UNIT, archer);
        totalUnits++;
        if (archersQueue.Count == 0)
        {
            setStatus(EntityStatus.IDLE);
            _createStatus = createArcherStatus.IDLE;
        }
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    override public void Awake()
    {
        

        _isReady = false;
        _nextUpdate = 0;
        _xDisplacement = 0;
        _yDisplacement = 0;
        _info = Info.get.of(race, type);
        totalUnits = 0;
        _unitRotation = transform.rotation;
        unitInfo = Info.get.of(this.race, UnitTypes.CIVIL);
        _makingNewArcher = false;
        archersQueue = new Queue<Action>();
        _constructionTime = 10;// send this to JSON
        _entity = this.GetComponent<IGameEntity>();
        _createStatus = createArcherStatus.DISABLED;
        // Call Building start
        base.Awake();
    }

    override public void Start()
    {
        // Setup base
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
        if (status == EntityStatus.IDLE)
        {
            _isReady = true;
            _createStatus = createArcherStatus.IDLE;
        }
       
                
        if (_createStatus == createArcherStatus.RUN)
        {
            if (Time.time > _constructionTime)
            {

                Action startNewArcher = archersQueue.Dequeue();
                startNewArcher();             
            }

        }

    }
}


