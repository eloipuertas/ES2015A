using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum BuildingModifier { RESISTANCE, WOUNDS };

public interface IBuildingAbility : IAction
{
    T getModifier<T>(BuildingModifier modifier);
}
