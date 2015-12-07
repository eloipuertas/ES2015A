using System;
using Storage;
using Utils;

public interface IBuilding : IGameEntity, IBaseActor
{
    bool addUnitQueue(UnitTypes type);
}
