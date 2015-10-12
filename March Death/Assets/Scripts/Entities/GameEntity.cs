using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Storage;
using Utils;

using UnityEngine;


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

    protected EntityAbility _accumulatedModifier;
    public R accumulatedModifier<R>() where R : EntityAbility
    {
        return (R)_accumulatedModifier;
    }

    public void onAbilityToggled(System.Object obj)
    {
        if (info.isUnit)
        {
            ((UnitAbility)_accumulatedModifier).weaponAbilityModifier =
                activeAbilities().Select(ability => ability.info<UnitAbility>().weaponAbilityModifier).Sum();

            ((UnitAbility)_accumulatedModifier).projectileAbilityModifier =
                activeAbilities().Select(ability => ability.info<UnitAbility>().projectileAbilityModifier).Sum();

            ((UnitAbility)_accumulatedModifier).resistanceModifier =
                activeAbilities().Select(ability => ability.info<UnitAbility>().resistanceModifier).Sum();

            ((UnitAbility)_accumulatedModifier).strengthModifier =
                activeAbilities().Select(ability => ability.info<UnitAbility>().strengthModifier).Sum();

            ((UnitAbility)_accumulatedModifier).woundsModifier =
                activeAbilities().Select(ability => ability.info<UnitAbility>().woundsModifier).Sum();

            ((UnitAbility)_accumulatedModifier).attackRateModifier =
                activeAbilities().Select(ability => ability.info<UnitAbility>().attackRateModifier).Sum();

            ((UnitAbility)_accumulatedModifier).movementRateModifier =
                activeAbilities().Select(ability => ability.info<UnitAbility>().movementRateModifier).Sum();
        }
        else if (info.isBuilding)
        {
            ((BuildingAbility)_accumulatedModifier).resistanceModifier =
                activeAbilities().Select(ability => ability.info<BuildingAbility>().resistanceModifier).Sum();

            ((BuildingAbility)_accumulatedModifier).woundsModifier =
                activeAbilities().Select(ability => ability.info<BuildingAbility>().woundsModifier).Sum();
        }
        else if (info.isResource)
        {

        }
    }

    protected List<Ability> _abilities = new List<Ability>();
    public Ability getAbility(string name)
    {
        foreach (Ability ability in _abilities)
        {
            if (ability.info<EntityAbility>().name.Equals(name))
            {
                return ability;
            }
        }

        throw new ArgumentException("Invalid action " + name + "requested");
    }

    public bool hasAbility(string name)
    {
        return _abilities.Where(ability => ability.info<EntityAbility>().name.Equals(name)).Count() > 0;
    }

    public List<Ability> activeAbilities()
    {
        return _abilities.Where(ability => ability.isActive).ToList();
    }

    /// <summary>
    /// Iterates all abilities on the
    /// </summary>
    protected void setupAbilities()
    {
        if (info.isUnit)
        {
            _accumulatedModifier = new UnitAbility();
        }
        else if (info.isBuilding)
        {
            _accumulatedModifier = new BuildingAbility();
        }
        else
        {
            _accumulatedModifier = new ResourceAbility();
        }

        foreach (EntityAbility ability in info.abilities)
        {
            // Try to get class with this name
            string abilityName = ability.name.Replace(" ", "");
            Ability newAbility = null;

            try
            {
                var constructor = Type.GetType(abilityName).
                    GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(UnitAbility), typeof(GameObject) }, null);
                if (constructor == null)
                {
                    // Invalid constructor, use GenericAbility
                    newAbility = new GenericAbility(ability, gameObject);
                }
                else
                {
                    // Class found, use that!
                    newAbility = (Ability)constructor.Invoke(new object[2] { ability, gameObject });
                }
            }
            catch (Exception /*e*/)
            {
                // No such class, use the GenericAbility class
                newAbility = new GenericAbility(ability, gameObject);
            }

            newAbility.register(Ability.Actions.ENABLED, onAbilityToggled);
            newAbility.register(Ability.Actions.DISABLED, onAbilityToggled);

            _abilities.Add(newAbility);
        }
    }

    public override void Start()
    {
        base.Start();
        setupAbilities();
    }

    public override void Update()
    {
        foreach (Ability ability in _abilities)
        {
            ability.Update();
        }
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
            int projectileAbility = from.info.unitAttributes.projectileAbility +
                from.accumulatedModifier<UnitAbility>().projectileAbilityModifier;

            return dice > 1 && (projectileAbility + dice >= 7);
        }

        // Buildings always get hit
        if (!_info.isUnit)
        {
            return true;
        }

        int attackerAbility = Math.Max(10, from.info.unitAttributes.weaponAbility +
            from.accumulatedModifier<UnitAbility>().weaponAbilityModifier);

        int defenderAbility = Math.Max(10, info.unitAttributes.weaponAbility +
            accumulatedModifier<UnitAbility>().weaponAbilityModifier);

        return HitTables.meleeHit[attackerAbility, defenderAbility] <= dice;
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

        int attackerStrength = Math.Max(10, from.info.unitAttributes.strength +
            from.accumulatedModifier<UnitAbility>().weaponAbilityModifier);

        int defenderResistance = Math.Max(10, info.unitAttributes.resistance +
            accumulatedModifier<UnitAbility>().resistanceModifier);

        int dice = Utils.D6.get.rollOnce();
        return HitTables.wounds[attackerStrength, defenderResistance] <= dice;
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
        if (_status == EntityStatus.DEAD || _status == EntityStatus.DESTROYED)
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
            _status = info.isUnit ? EntityStatus.DEAD : EntityStatus.DESTROYED;
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
