using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum Modifier { WEAPON, PROJECTILE, STRENGTH, RESISTANCE, WOUNDS, ATTACKRATE, MOVEMENTRATE };

public abstract class Ability
{
    public abstract Storage.EntityAction info { get; }

    public abstract bool isActive { get; }
    public abstract bool isUsable { get; }

    public abstract void enable();
    public abstract void disable();

    public void toggle()
    {
        if (isActive)
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
