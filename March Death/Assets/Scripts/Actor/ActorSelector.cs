using System;
using UnityEngine;

public class ActorSelector
{
    public Func<GameObject, bool> registerCondition = x => true;
    public Func<GameObject, bool> fireCondition = x => true;
}
