using Storage;

public enum EntityStatus { IDLE, MOVING, ATTACKING, DEAD };

public interface IGameEntity
{
    EntityInfo info { get; }
    EntityStatus status { get; }

    float damagePercentage { get; }
    float healthPercentage { get; }

    IAction getAction(string name);

    Unit toUnit();
    Building toBuilding();
}
