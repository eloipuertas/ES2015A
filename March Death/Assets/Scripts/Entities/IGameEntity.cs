using System;
﻿using Storage;

public enum EntityStatus { IDLE, MOVING, ATTACKING, DEAD, DESTROYED };

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
