using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;


/// <summary>
/// Unit base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Unit : Utils.Actor<Unit.Actions>, IGameEntity
{
    public enum Actions { MOVEMENT_START, MOVEMENT_END, DAMAGED, DIED };

    public Unit() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public Races race = Races.MEN;
    public UnitTypes type = UnitTypes.HERO;

    /// <summary>
    /// If in battle, this is the target and last attack time
    /// </summary>
    private Unit _target = null;
    private double _lastAttack = 0;

    /// <summary>
    /// Point to move to
    /// </summary>
    private Vector3 _movePoint;

    /// <summary>
    /// List of ability objects of this unit
    /// </summary>
    private List<IUnitAbility> _abilities;

    /// <summary>
    /// Contains all static information of the Unit.
    /// That means: max health, damage, defense, etc.
    /// </summary>
    private UnitInfo _info;
    public EntityInfo info
    {
        get
        {
            return _info;
        }
    }

    /// <summary>
    /// Returns current status of the Unit
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
    /// Civil units might need this to acount how many *items* they are carrying.
    /// </summary>
    public int usedCapacity { get; set; }

    /// <summary>
    /// Returns the number of wounds a unit received
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
            return (_info.attributes.wounds - _woundsReceived) * 100f / _info.attributes.wounds;
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
    /// Returns true in case an attack will land on this unit
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
            return dice > 1 && (_info.attributes.projectileAbility + dice >= 7);
        }

        return HitTables.meleeHit[from._info.attributes.weaponAbility, _info.attributes.weaponAbility] <= dice;
    }

    /// <summary>
    /// Retuns true if an attack will cause wounds to this unit
    /// </summary>
    /// <param name="from">Attacker</param>
    /// <returns>True if causes wounds, false otherwise</returns>
    private bool willAttackCauseWounds(Unit from)
    {
        int dice = Utils.D6.get.rollOnce();
        return HitTables.wounds[from._info.attributes.strength, _info.attributes.resistance] <= dice;
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
        if (_woundsReceived == _info.attributes.wounds)
        {
            _status = EntityStatus.DEAD;
            _target = null;

            fire(Actions.DIED);
        }
    }

    /// <summary>
    /// Called once our target dies. It may be used to update unit IA
    /// </summary>
    /// <param name="gob"></param>
    private void onTargetDied(GameObject gob)
    {
        // TODO: Our target died, select next? Do nothing?
        _status = EntityStatus.IDLE;
    }

    /// <summary>
    /// Sets up our attack target, registers callback for its death and
    /// updates our state
    /// </summary>
    /// <param name="unit"></param>
    public void attackTarget(Unit unit)
    {
        _target = unit;
        _target.register(Actions.DIED, onTargetDied);
        _status = EntityStatus.ATTACKING;
    }

    /// <summary>
    /// Stops attacking the target and goes back to an IDLE state
    /// </summary>
    public void stopAttack()
    {
        _target.unregister(Actions.DIED, onTargetDied);
        // TODO: Maybe we should not set it to null? In case we want to attack it again
        _target = null;
        _status = EntityStatus.IDLE;
    }

    /// <summary>
    /// Starts moving the unit towards a point on a terrain
    /// </summary>
    /// <param name="movePoint">Point to move to</param>
    public void moveTo(Vector3 movePoint)
    {
        _movePoint = movePoint;

        float distance = Vector3.Distance(movePoint, transform.position);
        if (distance > 1.50f)
        {
            // TODO: SMOOTH TURNING
            Quaternion targetRotation = Quaternion.LookRotation(movePoint - transform.position);
            targetRotation.x = 0;   // lock rotation on x-axis
            transform.rotation = targetRotation;
        }

        _status = EntityStatus.MOVING;
        fire(Actions.MOVEMENT_START);
    }

    /// <summary>
    /// Iterates all abilities on the
    /// </summary>
    private void setupAbilities()
    {
        _abilities = new List<IUnitAbility>();

        foreach (UnitAbility ability in _info.actions)
        {
            // Try to get class with this name
            string abilityName = ability.name.Replace(" ", "");

            var constructor = Type.GetType(abilityName).
                GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(UnitAbility), typeof(GameObject) }, null);
            if (constructor == null)
            {
                // No such class, use the GenericAbility class
                _abilities.Add(new GenericAbility(ability));
            }
            else
            {
                // Class found, use that!
                _abilities.Add((IUnitAbility)constructor.Invoke(new object[2] { ability, gameObject }));
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
    /// <returns>Always returns a valid IUnitAbility (IAction)</returns>
    public IAction getAction(string name)
    {
        foreach (IUnitAbility ability in _abilities)
        {
            if (ability.info.name.Equals(name))
            {
                return ability;
            }
        }

        throw new ArgumentException("Invalid action " + name + "requested");
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    void Start()
    {
        _status = EntityStatus.IDLE;
        _info = Info.get.of(race, type);
        setupAbilities();
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    void Update()
    {
#if TEST_INPUT
        if (Input.GetMouseButtonDown(0))
        {
            Camera mainCamera = Camera.main;

            // We need to actually hit an object
            RaycastHit hit;
            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100))
                return;

            // We need to hit something (with a collider on it)
            if (!hit.transform)
                return;

            // Get input vector from kayboard or analog stick and make it length 1 at most
            moveTo(hit.point);
        }
#endif
        switch (_status)
        {
            case EntityStatus.ATTACKING:
                if (_target != null)
                {
                    if (Time.time - _lastAttack >= (1f / _info.attributes.attackRate))
                    {
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
        switch (_status)
        {
            case EntityStatus.MOVING:
                // TODO: Steps on the last sector are smoothened due to distance being small
                // Althought it's an unintended behaviour, it may be interesating to leave it as is
                transform.position = Vector3.MoveTowards(transform.position, _movePoint, Time.fixedDeltaTime * _info.attributes.movementRate);

                // If distance is lower than 0.5, stop movement
                if (Vector3.Distance(transform.position, _movePoint) <= 0.5f)
                {
                    _status = EntityStatus.IDLE;
                    fire(Actions.MOVEMENT_END);
                }
                break;
        }
    }

    /// <summary>
    /// Casts this IGameEntity to Unity (pointless if already unit)
    /// </summary>
    /// <returns>Object casted to Unit</returns>
    public Unit toUnit() { return this;  }

    /// <summary>
    /// Returns NULL as this cannot be converted to Building
    /// </summary>
    /// <returns>Always null</returns>
    public Building toBuilding() { return null; }

}
