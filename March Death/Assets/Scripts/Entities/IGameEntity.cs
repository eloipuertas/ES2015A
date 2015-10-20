using System;
﻿using Storage;
using Utils;

// Values are set only for clarity purposes on the Animator
// Once DEAD/DESTROYED, you can not go back to any state!
public enum EntityStatus
{
    BUILDING_PHASE_1 = 0,
    BUILDING_PHASE_2 = 1,
    BUILDING_PHASE_3 = 2,
    IDLE = 4,
    MOVING = 5,
    ATTACKING = 6,
    DEAD = 7,
    DESTROYED = 8,
    WORKING = 9
};

public interface IGameEntity : IBaseActor
{
    EntityInfo info { get; }
    EntityStatus status { get; }

    int wounds { get; }
    float damagePercentage { get; }
    float healthPercentage { get; }

    UnityEngine.Transform getTransform();
    Ability getAbility(string name);

    Races getRace();
    E getType<E>() where E : struct, IConvertible;

    IKeyGetter registerFatalWounds(Action<System.Object> func);
    IKeyGetter unregisterFatalWounds(Action<System.Object> func);

    void receiveAttack(Unit from, bool isRanged);

    void doIfUnit(Action<Unit> callIfTrue);
}
