using System;
using Storage;

class GenericResourceAbility : IResourceAbility
{
    private bool _enabled = false;
    public bool isActive
    {
        get
        {
            return _enabled;
        }
    }

    private ResourceAbility _info = null;
    public EntityAction info
    {
        get
        {
            return _info;
        }
    }

    public bool isUsable
    {
        get
        {
            return true;
        }
    }

    public GenericResourceAbility(ResourceAbility info) { _info = info; }

    public void disable() { _enabled = false; }
    public void enable() { _enabled = true; }
    public void toggle()
    {
        if (_enabled)
        {
            disable();
        }
        else
        {
            enable();
        }
    }

    public T getModifier<T>(Modifier modifier)
    {
        if (!_enabled)
        {
            return (T)Convert.ChangeType(0, typeof(T));
        }

        switch (modifier)
        {
            case Modifier.WEAPON: return (T)Convert.ChangeType(_info.weaponAbilityModifier, typeof(T));
            case Modifier.PROJECTILE: return (T)Convert.ChangeType(_info.projectileAbilityModifier, typeof(T));
            case Modifier.STRENGTH: return (T)Convert.ChangeType(_info.strengthModifier, typeof(T));
            case Modifier.RESISTANCE: return (T)Convert.ChangeType(_info.resistanceModifier, typeof(T));
            case Modifier.ATTACKRATE: return (T)Convert.ChangeType(_info.attackRateModifier, typeof(T));
            case Modifier.MOVEMENTRATE: return (T)Convert.ChangeType(_info.movementRateModifier, typeof(T));
            default: throw new ArgumentException("Modifier " + modifier + " not found");
        }
    }
}
