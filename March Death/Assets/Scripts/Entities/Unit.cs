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

    /// <summary>
    /// Set to true if we are following our target (ie. attacking but not in range)
    /// </summary>
    private bool followingTarget = false;

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
    /// Civil units might need this to acount how many *items* they are carrying.
    /// </summary>
    public int usedCapacity { get; set; }

    /// <summary>
    /// Called once our target dies. It may be used to update unit IA
    /// </summary>
    /// <param name="gob"></param>
    private void onTargetDied(System.Object obj)
    {
        // TODO: Our target died, select next? Do nothing?
        setStatus(EntityStatus.IDLE);
    }

    private void onTargetHidden(System.Object obj)
    {
        moveTo(((GameObject)obj).transform.position);
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

    public override IKeyGetter registerFatalWounds(Action<object> func)
    {
        return register(Actions.DIED, func);
    }

    public override IKeyGetter unregisterFatalWounds(Action<object> func)
    {
        return unregister(Actions.DIED, func);
    }

    /// <summary>
    /// Sets up our attack target, registers callback for its death and
    /// updates our state
    /// </summary>
    /// <param name="unit"></param>
    public bool attackTarget<A>(GameEntity<A> entity) where A : struct, IConvertible
    {
        // Note: Cast is redundant but avoids warning
        if (_target != (IGameEntity)entity)
        {
            _auto += entity.registerFatalWounds(onTargetDied);
            _auto += entity.GetComponent<FOWEntity>().register(FOWEntity.Actions.HIDDEN, onTargetHidden);
            _target = entity;

            if (Vector3.Distance(entity.transform.position, transform.position) <= info.unitAttributes.attackRange)
            {
                setStatus(EntityStatus.ATTACKING);
                return true;
            }
            else
            {
                followingTarget = true;
                setStatus(EntityStatus.MOVING);
            }
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
            _auto -= _target.unregisterFatalWounds(onTargetDied);
            _auto -= _target.getGameObject().GetComponent<FOWEntity>().unregister(FOWEntity.Actions.HIDDEN, onTargetHidden);
            _target = null;
        }

        setStatus(EntityStatus.IDLE);
    }

    public void faceTo(Vector3 point)
    {
        // TODO: SMOOTH TURNING
        Quaternion targetRotation = Quaternion.LookRotation(point - transform.position);
        targetRotation.x = 0;   // lock rotation on x-axis
        targetRotation.z = 0;   // lock rotation on z-axis
        transform.rotation = targetRotation;
    }

    /// <summary>
    /// Starts moving the unit towards a point on a terrain
    /// </summary>
    /// <param name="movePoint">Point to move to</param>
    public void moveTo(Vector3 movePoint)
    {
        _movePoint = movePoint;
        faceTo(movePoint);

        followingTarget = false;
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

        // Call GameEntity start
        base.Awake();

        // Set the status
        setStatus(EntityStatus.IDLE);
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    public override void Update()
    {
        base.Update();

#if TEST_INPUT
        if (Input.GetMouseButtonDown(0))
        {
            Camera mainCamera = Camera.main;

            // We need to actually hit an object
            RaycastHit hit;
            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 1000))
                return;

            // We need to hit something (with a collider on it)
            if (!hit.transform)
                return;

            // Get input vector from kayboard or analog stick and make it length 1 at most
            moveTo(hit.point);
        }
#endif

        float resourcesElapsed = Time.time - _lastResourcesUpdate;
        if (resourcesElapsed > RESOURCES_UPDATE_INTERVAL)
        {
            _lastResourcesUpdate = Time.time;

            // Food is always consumed
            float foodConsumed = info.unitAttributes.foodConsumption * resourcesElapsed;
            float goldConsumed = info.unitAttributes.goldConsumption * resourcesElapsed;
            float goldProduced = 0;

            if (info.isCivil && role == Roles.PRODUCING)
            {
                goldProduced = info.unitAttributes.goldProduction * resourcesElapsed;
                goldConsumed = 0;
            }

            BasePlayer.getOwner(this).resources.AddAmount(WorldResources.Type.GOLD, goldProduced);
            BasePlayer.getOwner(this).resources.SubstractAmount(WorldResources.Type.GOLD, goldConsumed);
            BasePlayer.getOwner(this).resources.SubstractAmount(WorldResources.Type.FOOD, foodConsumed);
        }

        switch (status)
        {
            case EntityStatus.ATTACKING:
                if (_target != null)
                {
                    // Check if we are still in range
                    if (Vector3.Distance(_target.getTransform().position, transform.position) > info.unitAttributes.attackRange)
                    {
                        followingTarget = true;
                        setStatus(EntityStatus.MOVING);
                    }
                    // Check if we already have to attack
                    else if (Time.time - _lastAttack >= (1f / info.unitAttributes.attackRate))
                    {
                        _lastAttack = Time.time;

                        // TODO: Ranged attacks!
                        _target.receiveAttack(this, false);
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
                // TODO: Steps on the last sector are smoothened due to distance being small
                // Althought it's an unintended behaviour, it may be interesating to leave it as is
                Vector3 destination = _movePoint;

                if (followingTarget)
                {
                    destination = _target.getTransform().position;
                    faceTo(destination);
                }

                transform.position = Vector3.MoveTowards(transform.position, destination, Time.fixedDeltaTime * info.unitAttributes.movementRate);

                // If distance is lower than 0.5, stop movement
                if (followingTarget && Vector3.Distance(transform.position, destination) <= info.unitAttributes.attackRange)
                {
                    setStatus(EntityStatus.ATTACKING);
                }

                if (Vector3.Distance(transform.position, _movePoint) <= 0.5f)
                {
                    setStatus(EntityStatus.IDLE);
                    fire(Actions.MOVEMENT_END);
                }
                break;
        }
    }
}
