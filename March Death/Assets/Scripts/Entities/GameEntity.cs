using System;
using System.Collections.Generic;
using Storage;
using Utils;

public abstract class GameEntity<T> : Actor<T>, IGameEntity where T : struct, IConvertible
{
    protected EntityInfo _info;
    public EntityInfo info
    {
        get
        {
            return _info;
        }
    }
    
    /// <summary>
    /// Returns the number of wounds received
    /// </summary>
    protected int _woundsReceived;
    public int wounds
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
            return (info.unitAttributes.wounds - _woundsReceived) * 100f / info.unitAttributes.wounds;
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
    /// Returns current status of the Unit
    /// </summary>
    protected EntityStatus _status;
    public EntityStatus status
    {
        get
        {
            return _status;
        }
    }

    protected List<IAction> _actions = new List<IAction>();
    public IAction getAction(string name)
    {
        foreach (IAction ability in _actions)
        {
            if (ability.info.name.Equals(name))
            {
                return ability;
            }
        }

        throw new ArgumentException("Invalid action " + name + "requested");
    }

    protected abstract void setupActions();

    public override void Start()
    {
        base.Start();
        setupActions();
    }



    /// <summary>
    /// Returns true in case an attack will land on this unit
    /// </summary>
    /// <param name="from">Unit which attacked</param>
    /// <param name="isRanged">Set to true in case the attack is range, false if melee</param>
    /// <returns>True if it hits, false otherwise</returns>
    protected bool willAttackLand(Unit from, bool isRanged = false)
    {
        int dice = Utils.D6.get.rollSpecial();

        if (isRanged)
        {
            // TODO: Specil units (ie gigants) and distance!
            return dice > 1 && (from.info.unitAttributes.projectileAbility + dice >= 7);
        }

        // Buildings always get hit
        if (!_info.isUnit)
        {
            return true;
        }

        return HitTables.meleeHit[from.info.unitAttributes.weaponAbility, info.unitAttributes.weaponAbility] <= dice;
    }

    /// <summary>
    /// Retuns true if an attack will cause wounds to this unit
    /// </summary>
    /// <param name="from">Attacker</param>
    /// <returns>True if causes wounds, false otherwise</returns>
    protected bool willAttackCauseWounds(Unit from)
    {
        // Buildings always get damage
        if (!_info.isUnit)
        {
            return true;
        }

        int dice = Utils.D6.get.rollOnce();
        return HitTables.wounds[from.info.unitAttributes.strength, info.unitAttributes.resistance] <= dice;
    }

    protected abstract void onReceiveDamage();
    protected abstract void onFatalWounds();

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
            onReceiveDamage();
        }

        // Check if we are dead
        if (_woundsReceived == info.unitAttributes.wounds)
        {
            _status = EntityStatus.DEAD;
            onFatalWounds();
        }
    }

    public Building toBuilding()
    {
        return this as Building;
    }

    public Resource toResource()
    {
        return this as Resource;
    }

    public Unit toUnit()
    {
        return this as Unit;
    }
}
