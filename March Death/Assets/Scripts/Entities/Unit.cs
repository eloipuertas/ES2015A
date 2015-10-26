using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Storage;


/// <summary>
/// Unit base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Unit : GameEntity<Unit.Actions>
{
    public enum Actions { MOVEMENT_START, MOVEMENT_END, DAMAGED, DIED };
    public enum Roles { PRODUCING, WANDERING };

    public Unit() { }

    /// <summary>
    /// Interval between resources update in miliseconds
    /// </summary>
    const int RESOURCES_UPDATE_INTERVAL = 5000;

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
    private float _distanceToTarget = 0;

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
    private NavMeshAgent _navAgent;

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
        // Move to last known position (ie. current position)
        moveTo(((GameObject)obj).transform.position);
        _target = null;
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
        _distanceToTarget = Vector3.Distance(_target.getTransform().position, transform.position);
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
            _target = entity;

            // Update distance for immediate usage (ie. canDoRangedAttack)
            updateDistanceToTarget();

            // Update status and return
            setStatus(EntityStatus.ATTACKING);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Stops attacking the target and goes back to an IDLE state
    /// </summary>
    public void stopAttack()
    {
        if (_target != null)
        {
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
    public void moveTo(Vector3 movePoint)
    {
        _followingTarget = false;
        _movePoint = movePoint;
        _navAgent.SetDestination(movePoint);

        setStatus(EntityStatus.MOVING);
        fire(Actions.MOVEMENT_START);
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

        // Set the status
        setStatus(EntityStatus.IDLE);
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public void Start()
    {
        activateFOWEntity();

        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.speed = _info.unitAttributes.movementRate;
        _navAgent.acceleration = info.unitAttributes.movementRate * 2.5f;
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
        }

        // Status dependant functionality
        switch (status)
        {
            // If we are attacking and have target
            case EntityStatus.ATTACKING:
                if (_target != null)
                {
                    // Look at it
                    faceTo(_target.getTransform().position);

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
                if (_followingTarget && _distanceToTarget <= currentAttackRange())
                {
                    _navAgent.Stop();
                    setStatus(EntityStatus.ATTACKING);
                    return;
                }

                if (!_navAgent.pathPending && _navAgent.hasPath)
                {
                    float dist = _navAgent.remainingDistance;

                    if (dist != Mathf.Infinity && _navAgent.pathStatus == NavMeshPathStatus.PathComplete && _navAgent.remainingDistance == 0)
                    {
                        if (!_followingTarget)
                        {
                            Debug.Log("fire(Actions.MOVEMENT_END)");
                            setStatus(EntityStatus.IDLE);
                            fire(Actions.MOVEMENT_END);
                        }
                        else
                        {
                            Debug.LogWarning("NavMesh not stopped at attack range... AttackRange = " + currentAttackRange());
                        }
                    }
                }
                
                // Update destination if we are following
                if (_followingTarget)
                {
                    _navAgent.SetDestination(_target.getTransform().position);
                }
                break;
        }
    }
}
