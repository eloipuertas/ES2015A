using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum BuildingModifier { RESISTANCE, WOUNDS, STORESIZE, MAXUNITS, PRODUCTIONRATE };

public interface IBuildingAbility : IAction
{
    T getModifier<T>(BuildingModifier modifier);
}
