using System;
﻿using Storage;

// Values are set only for clarity purposes on the Animator
// Once DEAD/DESTROYED, you can not go back to any state!
public enum EntityStatus
{
    IDLE = 0,
    MOVING = 1,
    ATTACKING = 2,
    DEAD = 3,
    DESTROYED = 4
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
