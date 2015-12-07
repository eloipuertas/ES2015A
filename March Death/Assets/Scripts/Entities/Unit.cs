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
    public enum Actions { CREATED, MOVEMENT_START, MOVEMENT_END, DAMAGED, EAT, DIED, STAT_OUT, TARGET_TERMINATED, HEALTH_UPDATED };
    public enum Gender { MALE, FEMALE }

    private EntityStatus _defaultStatus = EntityStatus.IDLE;
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

    public Unit() { }

    /// <summary>
    /// Interval between resources update in miliseconds
    /// </summary>
    const float RESOURCES_UPDATE_INTERVAL = 15.0f;

    /// <summary>
    /// Update follow distance when greater than this value
    /// Do note this values is the SQUARED (^2) value of the real distance
    /// </summary>
    const float SQR_UPDATE_DISTANCE = 75.0f;

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
    /// Edit this on the Prefab to set the Unit Gender
    /// </summary>
    public Gender gender = Gender.MALE;

    /// <summary>
    /// List of components needed to deactivate at Vanish() and activate at bringback()
    /// </summary>
    protected List<Component> unitConponents = new List<Component>();

    /// <summary>
    /// If in battle, this is the target and last attack time
    /// </summary>
    private IGameEntity _target = null;
    private bool _selfDefense = false;
    public float _distanceToTarget = 0;
    private Vector3 _attackPoint;
    private Vector3 _closestPointToTarget;

    private Vector3 _projectileEndPoint;
    private bool _projectileThrown = false;
    private GameObject _projectile;
    //private Helpers _Helpers;

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
    /// Point to move to
    /// </summary>
    private Vector3 _movePoint;

    /// <summary>
    /// NavAgent, used fot map navigation
    /// </summary>
    private DetourAgent _detourAgent;
    public DetourAgent Agent { get { return _detourAgent; } }

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

    public bool isImmobile
    {
        get
        {
            return info.unitAttributes.movementRate == 0;
        }
    }

    /// <summary>
    /// Max. euclidean distance to the target
    /// </summary>
    private Squad _squad;
    private SquadUpdater _squadUpdater;
    public Squad Squad
    {
        get
        {
            if (_squad == null)
            {
                _squadUpdater = gameObject.AddComponent<SquadUpdater>();
                _squadUpdater.Initialize(info.race);
                _squad = _squadUpdater.UnitsSquad;
                _squad.AddUnit(this);
            }

            return _squad;
        }
        set
        {
            if (_squadUpdater && value != _squad)
            {
                GameObject.Destroy(_squadUpdater);
            }

            _squad = value;
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
        setStatus(EntityStatus.IDLE);
        // TODO: After merge, I had a conflich in this line and I hesitated what to do
        fire(Actions.TARGET_TERMINATED, obj);

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
		base.onReceiveDamage ();
        fire(Actions.DAMAGED);
    }

    /// <summary>
    /// When wounds reach its maximum, thus unit dies, this is called
    /// </summary>
    protected override void onFatalWounds()
    {
        statistics.getNegative();
        fire(Actions.STAT_OUT, statistics);

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
    /// Starts unit travel to building resource
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    public bool goToBuilding(IGameEntity building)
    {

        _target = building;
        _followingTarget = true;
        _movePoint = building.getTransform().position;
        _detourAgent.MoveTo(_movePoint);
        setStatus(EntityStatus.MOVING);
        fire(Actions.MOVEMENT_START);
        updateDistanceToTarget();
        Debug.Log("Unit: GoToBuilding()");

        return true;
    }

    /// <summary>
    /// Sets up our attack target, registers callback for its death and
    /// updates our state.
    /// If target is in range, attaks it, otherwise starts moving towards it.
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>Returns true if target is in range, false otherwise</returns>
    public bool attackTarget<A>(GameEntity<A> entity, bool selfDefense) where A : struct, IConvertible
    {
        // Note: Cast is redundant but avoids warning
        if (_target != (IGameEntity)entity)
        {
            // Register for DEAD/DESTROYED and HIDDEN
            _auto += entity.registerFatalWounds(onTargetDied);
            _auto += entity.GetComponent<FOWEntity>().register(FOWEntity.Actions.HIDDEN, onTargetHidden);

            // if target has changed, hide old target health
            Selectable selectable = null;
            if (_target != null)
            {
            	selectable = _target.getGameObject().GetComponent<Selectable>();
            	selectable.NotAttackedEntity();
            }

            _target = entity;
            _selfDefense = selfDefense;

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

    public bool attackTarget(IGameEntity entity, bool selfDefense = false)
    {
        if (entity.info.isUnit)
        {
            return attackTarget((Unit)entity, selfDefense);
        }
        else if (entity.info.isBarrack)
        {
            return attackTarget((Barrack)entity, selfDefense);
        }
        else if (entity.info.isResource)
        {
            return attackTarget((Resource)entity, selfDefense);
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
        if (isImmobile)
        {
            return false;
        }

        _detourAgent.MoveTo(movePoint);

        _followingTarget = false;
        _target = null;
        _movePoint = movePoint;
        setStatus(EntityStatus.MOVING);
        fire(Actions.MOVEMENT_START);

        return true;
    }

    /// <summary>
    /// Vanish unit from game just disabling components attached.
    /// If some new component is added you must add it to this method
    /// and to bringback method too.
    /// </summary>
    ///
    public void vanish()
    {

        //Disable FOW
        if (GetComponent<FOWEntity>())
        {
            GetComponent<FOWEntity>().enabled = false;
        }
        //disable EntityMarker
        if (GetComponent<EntityMarker>())
        {
            GetComponent<EntityMarker>().enabled = false;
        }
        // Disable animator
        if (GetComponent<Animator>())
        {
            GetComponent<Animator>().enabled = false;
        }
        // Rigidbody can't be disabled. Must toggle detectCollisions and iskinematic
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().detectCollisions = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
        //Disable Selectable
        // TODO: can't disable square border and lifebar

        if (GetComponent<Selectable>())
        {
            GetComponent<Selectable>().enabled = false;
        }

        // Disable ligths if any
        if (GetComponent<Light>())
        {
            GetComponent<Light>().enabled = false;
        }
        //disable collider
        if(GetComponent<Collider>())
        {
            GetComponent<Collider>().enabled = false;
        }
        // Disable DetourAgent. Must remove form crowd first
        if (GetComponent<DetourAgent>())
        {
            GetComponent<DetourAgent>().RemoveFromCrowd();
            GetComponent<DetourAgent>().enabled = false;
        }
        // Disable render
        Component[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            r.enabled = false;
        }


    }
    /// <summary>
    /// Units vanished with vanish method needs to uses BringBack method in
    /// order to enable components.
    /// </summary>
    public void bringBack()
    {

        if (GetComponent<FOWEntity>())
        {
            GetComponent<FOWEntity>().enabled = true;
        }
        if (GetComponent<EntityMarker>())
        {
            GetComponent<EntityMarker>().enabled = true;
        }
        if (GetComponent<Animator>())
        {
            GetComponent<Animator>().enabled = true;
        }
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().detectCollisions = true;
            GetComponent<Rigidbody>().isKinematic = false;
        }
        if (GetComponent<Collider>())
        {
            GetComponent<Collider>().enabled = true;
        }

        if (GetComponent<Light>())
        {
            GetComponent<Light>().enabled = true;
        }
        if (GetComponent<DetourAgent>())
        {
            GetComponent<DetourAgent>().enabled = true;
        }
        Component[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            r.enabled = true;
        }
        if (GetComponent<Selectable>())
        {
            GetComponent<Selectable>().enabled = true;
        }
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

        // Get DetourAgent and set basic variables
        _detourAgent = GetComponent<DetourAgent>();
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Start()
    {
        // Setup base
        base.Start();

        // Set the status
        setStatus(DefaultStatus);

        activateFOWEntity();

        // Statistics available for both AI and Player
        GameObject gameController = GameObject.Find("GameController");
        ResourcesPlacer res_pl = gameController.GetComponent<ResourcesPlacer>();
        register(Actions.EAT, res_pl.onFoodConsumption);

        // Statistics only available to player
        if (Player.getOwner(this) == BasePlayer.player)
        {
            register(Actions.STAT_OUT, res_pl.onStatisticsUpdate);
            register(Actions.CREATED, res_pl.onStatisticsUpdate);

            float foodConsumption = info.unitAttributes.foodConsumption * RESOURCES_UPDATE_INTERVAL;
            statistics = new Statistics(WorldResources.Type.FOOD, RESOURCES_UPDATE_INTERVAL, foodConsumption);
            fire(Actions.CREATED, statistics);
        }

        // Set detour params (can't be done until Start is done)
        if (!isImmobile)
        {
            _detourAgent.MaxSpeed = info.unitAttributes.movementRate * 5;
            _detourAgent.MaxAcceleration = info.unitAttributes.movementRate * 20;
            _detourAgent.UpdateParams();
        }
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
            if (info.isCivil && status == EntityStatus.WORKING)
            {
                goldProduced = info.unitAttributes.goldProduction * resourcesElapsed;
                goldConsumed = 0;
            }

            // Update this unit resources
            BasePlayer.getOwner(this).resources.AddAmount(WorldResources.Type.GOLD, goldProduced);
            BasePlayer.getOwner(this).resources.SubstractAmount(WorldResources.Type.GOLD, goldConsumed);

            CollectableGood collectable = new CollectableGood();
            collectable.entity = this;
            collectable.goods = new Goods(); // Generate the goods the units eat
            collectable.goods.amount = foodConsumed;
            collectable.goods.type = Goods.GoodsType.FOOD;

            fire(Actions.EAT, collectable);
        }

        // Status dependant functionality
        switch (status)
        {
            // If we are attacking and have target
            case EntityStatus.ATTACKING:
                if (_target != null)
                {
                    // Look at it
                    if (!isImmobile)
                    {
                        faceTo(_closestPointToTarget);
                    }

                    // Check if we are still in range
                    if (_distanceToTarget > currentAttackRange())
                    {
                        if (!_selfDefense)
                        {
                            _followingTarget = true;
                            setStatus(EntityStatus.MOVING);
                        }
                        else
                        {
                            setStatus(EntityStatus.IDLE);
                        }
                    }
                    // Check if we already have to attack
                    else if (! _projectileThrown &&
                        (Time.time - _lastAttack >= (1f / info.unitAttributes.attackRate)))
                    {
                        if (canDoRangedAttack())
                        {
                            _projectile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            _projectile.AddComponent<Rigidbody>();
                            _projectile.transform.position = new Vector3(transform.position.x, transform.position.y + GetComponent<Collider>().bounds.size.y, transform.position.z);
                            _projectileEndPoint = new Vector3(_target.getTransform().position.x, _target.getTransform().position.y + _target.getGameObject().GetComponent<Collider>().bounds.size.y, _target.getTransform().position.z);
                            _projectileThrown = true;
                        }
                        else
                        {
                            _target.receiveAttack(this, canDoRangedAttack());
                        }

                        _lastAttack = Time.time;
                    }
                }
                break;
        }

        if (_projectileThrown) {
            //Find a new position proportionally closer to the end, based on the projectileSpeed
            Vector3 newPostion = Vector3.MoveTowards(_projectile.transform.position, _projectileEndPoint, info.unitAttributes.projectileSpeed * Time.deltaTime);

            //Move the object to the new position.
            _projectile.transform.position = newPostion;

            //Recalculate the remaining distance after moving.
            float sqrRemainingDistance = (_projectile.transform.position - _projectileEndPoint).sqrMagnitude;

            // If we reach the target...
            if (sqrRemainingDistance <= float.Epsilon) {
                List<IGameEntity> objectsInRadius = Helpers.getEntitiesNearPosition(_projectileEndPoint, info.unitAttributes.projectileRadius);

                // Should I prevent friendly fire?
                foreach (IGameEntity inRadiusObject in objectsInRadius.ToArray()) {
                    inRadiusObject.receiveAttack(this, canDoRangedAttack());
                }

                _projectileThrown = false;
                GameObject.Destroy(_projectile);
            }

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

    public override void setStatus(EntityStatus status)
    {
        if (!_followingTarget && base.status == EntityStatus.MOVING)
        {
            fire(Actions.MOVEMENT_END);
        }

        base.setStatus(status);
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
                    // If we are just civilian unit travelling to our own building resource.
                    // Check distance to target to know if we are close enough.
                    // When capture distance is reached resource building capture
                    // method is triggered.

                    if (_target.info.race == this.race)
                    {
                        if (_distanceToTarget <= _target.info.resourceAttributes.trapRange)

                        {
                            _detourAgent.ResetPath();
                            setStatus(EntityStatus.IDLE);
                            _followingTarget = false;
                            fire(Actions.MOVEMENT_END);
                            ((Resource)_target).trapUnit(this);
                            return;
                        }

                    }

                    // Update destination only if target has moved
                    Vector3 destination = _closestPointToTarget;

                    if (!isImmobile)
                    {
                        if ((destination - _movePoint).sqrMagnitude > SQR_UPDATE_DISTANCE)
                        {
                            // Try to predict next point!
                            _target.doIfUnit(u =>
                            {
                                if (!u.isImmobile)
                                {
                                    destination = _closestPointToTarget + u.Agent.Velocity.normalized * (float)Math.Sqrt(SQR_UPDATE_DISTANCE);
                                }
                            });

                            // Save move point
                            _movePoint = destination;
                            _detourAgent.MoveTo(destination);
                        }
                    }

                    // If we are already close enough, stop and attack
                    if (_distanceToTarget <= currentAttackRange())
                    {
                        _detourAgent.ResetPath();
                        setStatus(EntityStatus.ATTACKING);
                        _followingTarget = false;
                        return;
                    }
                    else if(!isImmobile)
                    {
                        // Save move point
                        _movePoint = destination;
                        _detourAgent.MoveTo(destination);
                    }
                }

                if (!_detourAgent.IsMoving)
                {
                    if (!_followingTarget)
                    {
                        _detourAgent.ResetPath();
                        setStatus(EntityStatus.IDLE);
                    }
                }

                break;
        }
    }
}
