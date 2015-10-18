using System;
﻿using Storage;

// Values are set only for clarity purposes on the Animator
// Once DEAD/DESTROYED, you can not go back to any state!
public enum EntityStatus
{
    BUILDING_PHASE_1 = 0,
    BUILDING_PHASE_2 = 1,
    BUILDING_PHASE_3 = 2,
    IDLE = 3,
    MOVING = 4,
    ATTACKING = 5,
    DEAD = 6,
    DESTROYED = 7
};

public interface IGameEntity
{
    EntityInfo info { get; }
    EntityStatus status { get; }

    int wounds { get; }
    float damagePercentage { get; }
    float healthPercentage { get; }

    Ability getAbility(string name);

    Races getRace();
    E getType<E>() where E : struct, IConvertible;

    void doIfUnit(Action<Unit> callIfTrue);
}
