using System;
using UnityEngine;
using Storage;
using Utils;

public interface IBuilding : IGameEntity, IBaseActor
{
    bool addUnitQueue(UnitTypes type);
    void setMeetingPoint(Vector3 position);
}
