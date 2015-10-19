using System;
using UnityEngine;

public class ActorSelector
{
    public Func<GameObject, bool> registerCondition;
    public Func<GameObject, bool> fireCondition;
}
