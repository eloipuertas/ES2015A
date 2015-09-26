using System;
using UnityEngine;

using Storage;

/// <summary>
/// Unit base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Unit : Utils.Actor<Unit.Actions>
{
    public enum Actions { DIED };
    public enum Status { IDLE, ATTACKING, DEAD };

    public Unit() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public Storage.Races race = Storage.Races.MEN;
    public Storage.Types type = Storage.Types.HERO;

    /// <summary>
    /// Contains all static information of the Unit.
    /// That means: max health, damage, defense, etc.
    /// </summary>
    public Storage.UnitInfo info { get; set; }

    /// <summary>
    /// If in battle, this is the target
    /// </summary>
    private Unit _target = null;
    private double _lastAttack = 0;

    /// <summary>
    /// Returns current status of the Unit
    /// </summary>
    private Status _status;
    private Status status
    {
        get
        {
            return _status;
        }
    }

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
    /// Contains percentual value of health (100-0%)
    /// </summary>
    public float woundsPct
    {
        get
        {
            return (info.attributes.wounds - _woundsReceived) * 100f / info.attributes.wounds;
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
            return dice > 1 && (info.attributes.projectileAbility + dice >= 7);
        }

        return HitTables.meleeHit[from.info.attributes.weaponAbility, info.attributes.weaponAbility] <= dice;
    }

    /// <summary>
    /// Retuns true if an attack will cause wounds to this unit
    /// </summary>
    /// <param name="from">Attacker</param>
    /// <returns>True if causes wounds, false otherwise</returns>
    private bool willAttackCauseWounds(Unit from)
    {
        int dice = Utils.D6.get.rollOnce();
        return HitTables.wounds[from.info.attributes.strength, info.attributes.resistance] <= dice;
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
        if (_status == Status.DEAD)
        {
            throw new InvalidOperationException("Can not receive damage while not alive");
        }

        // If it hits and produces damage, update wounds
        if (willAttackLand(from, isRanged) && willAttackCauseWounds(from))
        {
            _woundsReceived += 1;
        }

        // Check if we are dead
        if (_woundsReceived == info.attributes.wounds)
        {
            _status = Status.DEAD;
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
        _status = Status.IDLE;
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
        _status = Status.ATTACKING;
    }

    /// <summary>
    /// Stops attacking the target and goes back to an IDLE state
    /// </summary>
    public void stopAttack()
    {
        _target.unregister(Actions.DIED, onTargetDied);
        // TODO: Maybe we should not set it to null? In case we want to attack it again
        _target = null; 
        _status = Status.IDLE;
    }

    // Use this for initialization
    void Start ()
    {
        info = Info.get.unit(race, type);
        _status = Status.IDLE;
    }

    // Update is called once per frame
    void Update ()
    {
        if (_status == Status.ATTACKING && _target != null)
        {
            if (Time.time - _lastAttack >= (1f / (float)info.attributes.attack_rate))
            {
                // TODO: Ranged attacks!
                _target.receiveAttack(this, false);
            }
        }
    }

}
