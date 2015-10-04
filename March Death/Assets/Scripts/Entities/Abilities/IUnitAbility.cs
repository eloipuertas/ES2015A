using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum Modifier { WEAPON, PROJECTILE, STRENGTH, RESISTANCE, WOUNDS, ATTACKRATE, MOVEMENTRATE };

public interface IUnitAbility : IAction
{
    T getModifier<T>(Modifier modifier);
}
