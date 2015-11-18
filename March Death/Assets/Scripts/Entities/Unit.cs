using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Storage;
using Pathfinding;


/// <summary>
/// Unit base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Unit : GameEntity<Unit.Actions>
{
    public enum Actions { CREATED, MOVEMENT_START, MOVEMENT_END, DAMAGED, EAT, DIED, STAT_OUT };
    public enum Roles { PRODUCING, WANDERING };

    public Unit() { }

    /// <summary>
    /// Interval between resources update in miliseconds
    /// </summary>
    const float RESOURCES_UPDATE_INTERVAL = 15.0f;

    Statistics statistics;

    ///<sumary>
    /// Auto-unregister events when we are destroyed
    ///</sumary>
    AutoUnregister _auto;

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public UnitTypes type = UnitTypes.HERO;
    public override E getType<E>() { return (E)Convert.ChangeType(type, typeof(E)); }

    /// <summary>
    /// If in battle, this is the target and last attack time
    /// </summary>
    private IGameEntity _target = null;
    public float _distanceToTarget = 0;
    private Vector3 _attackPoint;
    private Vector3 _closestPointToTarget;

    /// <summary>
    /// Set to true if we are following our target (ie. attacking but not in range)
    /// </summary>
    private bool _followingTarget = false;

    /// <summary>
    /// Time holders for const updates
    /// </summary>
    private float _lastResourcesUpdate = 0;
    private float _lastAttack = 0;

    /// <summary>
    /// Get and set current role (mostly for CIVILS)
    /// </summary>
    public Roles role { get; set; }

    /// <summary>
    /// Point to move to
    /// </summary>
    private Vector3 _movePoint;

    /// <summary>
    /// NavAgent, used fot map navigation
    /// </summary>
    private DetourAgent _detourAgent;

    /// <summary>
    /// Can this unit perform ranged attacks?
    /// </summary>
    public bool isRanged
    {
        get
        {
            return info.unitAttributes.rangedAttackFurthest > 0;
        }
    }

    /// <summary>
    /// Max. euclidean distance to the target
    /// </summary>
    public float currentAttackRange()
    {
        if (isRanged)
        {
            if (_distanceToTarget > info.unitAttributes.rangedAttackNearest)
            {
                return info.unitAttributes.rangedAttackFurthest;
            }
        }

        // Either it is not ranged or it is too close to the target
        return info.unitAttributes.attackRange;
    }

    /// <summary>
    /// Called once our target dies. It may be used to update unit IA
    /// </summary>
    /// <param name="gob"></param>
    private void onTargetDied(System.Object obj)
    {
        // TODO: Our target died, select next? Do nothing?
        setStatus(EntityStatus.IDLE);
        _target = null;
    }

    /// <summary>
    /// Callback issued when a target hides in the FOW
    /// </summary>
    private void onTargetHidden(System.Object obj)
    {
        if (_followingTarget || status == EntityStatus.ATTACKING)
        {
            // Move to last known position (ie. current position)
            moveTo(((GameObject)obj).transform.position);
        }
    }

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
        fire(Actions.DIED);

        statistics.growth_speed *= -1;
        fire(Actions.STAT_OUT, statistics);
    }

    /// <summary>
    /// Registers for DEAD events
    /// </summary>
    /// <returns>Register result</returns>
    public override IKeyGetter registerFatalWounds(Action<object> func)
    {
        return register(Actions.DIED, func);
    }

    /// <summary>
    /// Unregisters of DEAD events
    /// </summary>
    /// <returns>Register result</returns>
    public override IKeyGetter unregisterFatalWounds(Action<object> func)
    {
        return unregister(Actions.DIED, func);
    }

    /// <summary>
    /// Calculates if this unit can do a ranged attack to its target
    /// </summary>
    /// <returns>True if in range, false otherwise</returns>
    public bool canDoRangedAttack()
    {
        if (!isRanged)
        {
            return false;
        }

        // Must be within Nearest <> Furthest
        return _distanceToTarget < info.unitAttributes.rangedAttackFurthest &&
            _distanceToTarget > info.unitAttributes.rangedAttackNearest;
    }

    /// <summary>
    /// Calculates what modifiers should be applied to ranged attacks, taking
    /// into account distance to target and (not yet) target dimensions
    /// </summary>
    /// <returns>RangedAbility modifier</returns>
    public override int computeRangedModifiers()
    {
        int modifier = 0;

        // If distance is greater than half the longest range, we penalize ability by 1
        if (_distanceToTarget > info.unitAttributes.rangedAttackFurthest / 2)
        {
            modifier -= 1;
        }

        return modifier;
    }

    /// <summary>
    /// Update current distance (taken from transform.position) to the target
    /// </summary>
    private void updateDistanceToTarget()
    {
        _attackPoint = closestPointTo(_target.getTransform().position);
        _attackPoint.y = transform.position.y;

        _closestPointToTarget = _target.closestPointTo(_attackPoint);
        _closestPointToTarget.y = _target.getTransform().position.y;

        _distanceToTarget = Vector3.Distance(_attackPoint, _closestPointToTarget);
    }

    /// <summary>
    /// Sets up our attack target, registers callback for its death and
    /// updates our state.
    /// If target is in range, attaks it, otherwise starts moving towards it.
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>Returns true if target is in range, false otherwise</returns>
    public bool attackTarget<A>(GameEntity<A> entity) where A : struct, IConvertible
    {
        // Note: Cast is redundant but avoids warning
        if (_target != (IGameEntity)entity)
        {
            // Register for DEAD/DESTROYED and HIDDEN
            _auto += entity.registerFatalWounds(onTargetDied);
            _auto += entity.GetComponent<FOWEntity>().register(FOWEntity.Actions.HIDDEN, onTargetHidden);

            // if target has changed, hide old target health
            Selectable selectable = null;
            if (_target != null) {
            	selectable = _target.getGameObject().GetComponent<Selectable>();
            	selectable.NotAttackedEntity();
            }

            _target = entity;
            
            // Show target health
            selectable = _target.getGameObject().GetComponent<Selectable>();
            selectable.AttackedEntity();

            // Update distance for immediate usage (ie. canDoRangedAttack)
            updateDistanceToTarget();

            // Update status and return
            setStatus(EntityStatus.ATTACKING);
            return true;
        }

        // TODO: Hack to get AI working
        return true;
    }

    public IGameEntity getTarget()
    {
        return _target;
    }

    public bool attackTarget(IGameEntity entity)
    {
        if (entity.info.isUnit)
        {
            return attackTarget((Unit)entity);
        }
        else if (entity.info.isBarrack)
        {
            return attackTarget((Barrack)entity);
        }
        else if (entity.info.isResource)
        {
            return attackTarget((Resource)entity);
        }

        throw new ArgumentException("Unkown entity type to attack");
    }

    /// <summary>
    /// Stops attacking the target and goes back to an IDLE state
    /// </summary>
    public void stopAttack()
    {
        if (_target != null)
        {
            // Hide target health
            Selectable selectable = _target.getGameObject().GetComponent<Selectable>();
            selectable.NotAttackedEntity();

            // Unregister all events
            _auto -= _target.unregisterFatalWounds(onTargetDied);
            _auto -= _target.getGameObject().GetComponent<FOWEntity>().unregister(FOWEntity.Actions.HIDDEN, onTargetHidden);
            _target = null;

            setStatus(EntityStatus.IDLE);
        }
    }

    /// <summary>
    /// Faces to where the point is
    /// </summary>
    /// <param name="point">Point to face to</param>
    /// <returns>Register result</returns>
    public void faceTo(Vector3 point)
    {
        Vector3 direction = (point - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    /// <summary>
    /// Starts moving the unit towards a point on a terrain
    /// </summary>
    /// <param name="movePoint">Point to move to</param>
    public bool moveTo(Vector3 movePoint)
    {
        _detourAgent.MoveTo(movePoint);

        _followingTarget = false;
        _target = null;
        _movePoint = movePoint;
        setStatus(EntityStatus.MOVING);
        fire(Actions.MOVEMENT_START);

        return true;
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Awake()
    {
        _info = Info.get.of(race, type);
        _auto = this;

        // Call GameEntity awake
        base.Awake();
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Start()
    {
        // Setup base
        base.Start();

        // Set the status
        setStatus(EntityStatus.IDLE);

        activateFOWEntity();

        GameObject gameInformationObject = GameObject.Find("GameInformationObject");
        GameObject gameController = GameObject.Find("GameController");
        ResourcesPlacer res_pl = gameController.GetComponent<ResourcesPlacer>();

        if (Player.getOwner(this).race.Equals(gameInformationObject.GetComponent<GameInformation>().GetPlayerRace()))
        {
            register(Actions.EAT, res_pl.onFoodConsumption);
            register(Actions.STAT_OUT, res_pl.onStatisticsUpdate);
            register(Actions.CREATED, res_pl.onStatisticsUpdate);
        }

        statistics = new Statistics(WorldResources.Type.FOOD, (int)RESOURCES_UPDATE_INTERVAL, -5);

        fire(Actions.CREATED, statistics);
        
        // Get DetourAgent and set basic variables
        _detourAgent = GetComponent<DetourAgent>();
        _detourAgent.SetMaxSpeed(info.unitAttributes.movementRate * 5);
        _detourAgent.SetMaxAcceleration(20.5f);
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    public override void Update()
    {
        base.Update();

        // Precompute distance to our target if we have target
        // Avoids computing it serveral (5+ times)
        if (_target != null)
        {
            updateDistanceToTarget();
        }

        // Calculate food consumption
        float resourcesElapsed = Time.time - _lastResourcesUpdate;
        if (resourcesElapsed > RESOURCES_UPDATE_INTERVAL)
        {
            _lastResourcesUpdate = Time.time;

            // Food is always consumed
            float foodConsumed = info.unitAttributes.foodConsumption * resourcesElapsed;
            float goldConsumed = info.unitAttributes.goldConsumption * resourcesElapsed;
            float goldProduced = 0;

            // Civils produce gold when working and doesn't consume it
            if (info.isCivil && role == Roles.PRODUCING)
            {
                goldProduced = info.unitAttributes.goldProduction * resourcesElapsed;
                goldConsumed = 0;
            }

            // Update this unit resources
            BasePlayer.getOwner(this).resources.AddAmount(WorldResources.Type.GOLD, goldProduced);
            BasePlayer.getOwner(this).resources.SubstractAmount(WorldResources.Type.GOLD, goldConsumed);
            BasePlayer.getOwner(this).resources.SubstractAmount(WorldResources.Type.FOOD, foodConsumed);

            Goods goods = new Goods(); // Generate the goods the units eat
            goods.amount = 5;
            goods.type = Goods.GoodsType.FOOD;

            fire(Actions.EAT, goods);
        }

        // Status dependant functionality
        switch (status)
        {
            // If we are attacking and have target
            case EntityStatus.ATTACKING:
                if (_target != null)
                {
                    // Look at it
                    faceTo(_closestPointToTarget);

                    // Check if we are still in range
                    if (_distanceToTarget > currentAttackRange())
                    {
                        _followingTarget = true;
                        setStatus(EntityStatus.MOVING);
                    }
                    // Check if we already have to attack
                    else if (Time.time - _lastAttack >= (1f / info.unitAttributes.attackRate))
                    {
                        _lastAttack = Time.time;
                        _target.receiveAttack(this, canDoRangedAttack());
                    }
                }
                break;
        }

#if UNITY_EDITOR
        if (status == EntityStatus.ATTACKING)
        {
            Debug.DrawLine(transform.position, _target.getTransform().position, Color.yellow);
            Debug.DrawLine(_attackPoint, _closestPointToTarget, Color.red);
        }
        else if (status == EntityStatus.MOVING)
        {
            if (_followingTarget)
            {
                Debug.DrawLine(transform.position, _movePoint, Color.blue);
            }
            else
            {
                Debug.DrawLine(transform.position, _movePoint, Color.green);
            }
        }
#endif
    }

    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
        switch (status)
        {
            case EntityStatus.MOVING:
                // If we are already in range, start attacking
                // Must be called before MoveTowards because it uses _distanceToTarget, which
                // would be otherwise outdated
                if (_followingTarget)
                {
                    // Update destination only if target has moved
                    Vector3 destination = _closestPointToTarget;
                    if (destination != _movePoint)
                    {
                        _detourAgent.MoveTo(destination);
                        _movePoint = destination;
                    }

                    // If we are already close enough, stop and attack
                    if (_distanceToTarget <= currentAttackRange())
                    {
                        _detourAgent.ResetPath();
                        _followingTarget = false;
                        setStatus(EntityStatus.ATTACKING);
                        return;
                    }
                }

                if (!_detourAgent.IsMoving)
                {
                    if (!_followingTarget)
                    {
                        _detourAgent.ResetPath();
                        setStatus(EntityStatus.IDLE);
                        fire(Actions.MOVEMENT_END);
                    }
                    else
                    {
                        Debug.LogWarning("NavMesh not stopped at attack range... AttackRange = " + currentAttackRange());
                    }
                }
                
                break;
        }
    }
}
