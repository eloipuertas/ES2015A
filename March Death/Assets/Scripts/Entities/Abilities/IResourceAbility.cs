
// abilities for resource???
public enum ResourceModifier { WEAPON, PROJECTILE, STRENGTH, RESISTANCE, WOUNDS, ATTACKRATE, MOVEMENTRATE };

public interface IResourceAbility : IAction
{
    T getModifier<T>(Modifier modifier);
}
