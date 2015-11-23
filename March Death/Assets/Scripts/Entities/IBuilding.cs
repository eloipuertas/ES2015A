using System;
using Storage;

public interface IBuilding : IGameEntity
{
    bool addUnitQueue(UnitTypes type);
}
