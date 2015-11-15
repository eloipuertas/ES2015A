//#define DISABLE_ANIMATOR

using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Storage;
using Utils;

using UnityEngine;


public abstract class GameEntity<T> : Actor<T>, IGameEntity where T : struct, IConvertible
{
    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public Races race = Races.MEN;
    public Races getRace() { return race; }
    public abstract E getType<E>() where E : struct, IConvertible;

    protected Collider _collider;
    protected Terrain _terrain;

    /// <summary>
    /// Called when Start has been called
    /// </summary>
    public Action<GameObject> onStartDone = null;

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
            if (info.isBuilding)
            {
                return (info.buildingAttributes.wounds - _woundsReceived) * 100f / info.buildingAttributes.wounds;
            }
            else
            {
                return (info.unitAttributes.wounds - _woundsReceived) * 100f / info.unitAttributes.wounds;
            }
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
    private EntityStatus _status;
    public EntityStatus status
    {
        get
        {
            return _status;
        }
    }

#if !DISABLE_ANIMATOR
    /// <sumary>
    /// Triggers animations on the model
    /// </sumary>
    Animator _animator = null;
#endif

    protected EntityAbility _accumulatedModifier;
    public R accumulatedModifier<R>() where R : EntityAbility
    {
        return (R)_accumulatedModifier;
    }

    public void onAbilityToggled(System.Object obj)
    {
        Ability ability = (Ability)obj;
        int addOrSubs = ability.isActive ? 1 : -1;

        if (info.isUnit)
        {
            ((UnitAbility)_accumulatedModifier).weaponAbilityModifier =
                 addOrSubs * ability.info<UnitAbility>().weaponAbilityModifier;

            ((UnitAbility)_accumulatedModifier).projectileAbilityModifier =
                 addOrSubs * ability.info<UnitAbility>().projectileAbilityModifier;

            ((UnitAbility)_accumulatedModifier).resistanceModifier = addOrSubs *
                addOrSubs * ability.info<UnitAbility>().resistanceModifier;

            ((UnitAbility)_accumulatedModifier).strengthModifier = addOrSubs *
                addOrSubs * ability.info<UnitAbility>().strengthModifier;

            ((UnitAbility)_accumulatedModifier).woundsModifier =
                addOrSubs * ability.info<UnitAbility>().woundsModifier;

            ((UnitAbility)_accumulatedModifier).attackRateModifier =
                addOrSubs * ability.info<UnitAbility>().attackRateModifier;

            ((UnitAbility)_accumulatedModifier).movementRateModifier =
                addOrSubs * ability.info<UnitAbility>().movementRateModifier;
        }
        else if (info.isBuilding)
        {
            ((BuildingAbility)_accumulatedModifier).resistanceModifier =
                addOrSubs * ability.info<BuildingAbility>().resistanceModifier;

            ((BuildingAbility)_accumulatedModifier).woundsModifier =
                addOrSubs * ability.info<BuildingAbility>().woundsModifier;
        }
    }

    public UnityEngine.Transform getTransform()
    {
        return transform;
    }

    public UnityEngine.GameObject getGameObject()
    {
        return gameObject;
    }

    public Vector3 closestPointTo(Vector3 point)
    {
        return _collider.ClosestPointOnBounds(point);
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

    public override void Awake()
    {
        base.Awake();

#if !DISABLE_ANIMATOR
        // Get the Animator
        _animator = gameObject.GetComponent<Animator>();
#endif
    }

    public virtual void Start()
    {
        setupAbilities();

        _collider = GetComponent<Collider>();
        _terrain = Terrain.activeTerrain;
    }

    public void Destroy(bool immediately = false)
    {
        // TODO: Should this be automatically handled with events?
        FOWManager.Instance.removeEntity(this.GetComponent<FOWEntity>());

        // TODO: Should this be automatically handled with events?
        Selectable selectable = GetComponent<Selectable>();
        if (selectable.currentlySelected)
        {
            selectable.DeselectMe();
        }

        // TODO: Should this be automatically handled with events?
        BasePlayer.getOwner(this).removeEntity(this);

        // Play dead and/or destroy
        Destroy(this.gameObject, immediately ? 0.0f : 5.0f);
    }

    public override void Update()
    {
        foreach (Ability ability in _abilities)
        {
            ability.Update();
        }
    }

    /// <summary>
    /// Based on distance and target type computes how ranged ability is modified
    /// </summary>
    public virtual int computeRangedModifiers()
    {
        return 0;
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
            // TODO: Specil units (ie gigants)
            int projectileAbility = from.info.unitAttributes.projectileAbility +
                from.accumulatedModifier<UnitAbility>().projectileAbilityModifier +
                from.computeRangedModifiers();

            return dice > 1 && (projectileAbility + dice >= 7);
        }

        // Buildings always get hit
        if (!_info.isUnit)
        {
            return true;
        }

        int attackerAbility = Math.Min(10, from.info.unitAttributes.weaponAbility +
            from.accumulatedModifier<UnitAbility>().weaponAbilityModifier);

        int defenderAbility = Math.Min(10, info.unitAttributes.weaponAbility +
            accumulatedModifier<UnitAbility>().weaponAbilityModifier);

        return HitTables.meleeHit[attackerAbility - 1, defenderAbility - 1] <= dice;
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

        int attackerStrength = Math.Min(10, from.info.unitAttributes.strength +
            from.accumulatedModifier<UnitAbility>().weaponAbilityModifier);

        int defenderResistance = Math.Min(10, info.unitAttributes.resistance +
            accumulatedModifier<UnitAbility>().resistanceModifier);

        int dice = Utils.D6.get.rollOnce();

        return HitTables.wounds[attackerStrength - 1, defenderResistance - 1] <= dice;
    }

    protected abstract void onReceiveDamage();
    protected abstract void onFatalWounds();

    public abstract IKeyGetter registerFatalWounds(Action<System.Object> func);
    public abstract IKeyGetter unregisterFatalWounds(Action<System.Object> func);

    /// <summary>
    /// Automatically calculates if an attack will hit, and in case it
    /// does it updates the current state.
    /// </summary>
    /// <param name="from">Attacker</param>
    /// <param name="isRanged">True if the attack is ranged, false if melee</param>
    public void receiveAttack(Unit from, bool isRanged)
    {
        // Do not attack dead targets
        if (status == EntityStatus.DEAD || status == EntityStatus.DESTROYED)
        {
            throw new InvalidOperationException("Can not receive damage while not alive");
        }

        // If it hits and produces damage, update wounds
        bool hitAndWounds = willAttackLand(from, isRanged) && willAttackCauseWounds(from);
        if (hitAndWounds)
        {
            _woundsReceived += 1;
            onReceiveDamage();
        }

        // Check if we are dead
        if (_woundsReceived == info.attributes.wounds)
        {
            setStatus(info.isUnit ? EntityStatus.DEAD : EntityStatus.DESTROYED);
            onFatalWounds();
            Destroy();
        }

        // If we are a unit and doing nothing, attack back
        doIfUnit(unit =>
        {
            if (unit.status == EntityStatus.IDLE)
            {
                unit.attackTarget(from);
            }
        });
    }

    public void doIfUnit(Action<Unit> callIfTrue)
    {
        Unit unit = this as Unit;
        if (unit != null)
        {
            callIfTrue(unit);
        }
    }

    protected void setStatus(EntityStatus status)
    {
        _status = status;

#if !DISABLE_ANIMATOR
        //TODO: Hack to get this working for the sprint, why is _animator
        _animator.SetInteger("animation_state", (int)status);
#endif
    }

    protected void activateFOWEntity()
    {
        FOWEntity fe = gameObject.AddComponent<FOWEntity>();
        fe.Range = info.attributes.sightRange;
        fe.Activate(info.race);
    }
}
