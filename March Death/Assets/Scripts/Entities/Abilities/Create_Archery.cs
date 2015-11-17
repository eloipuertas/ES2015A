using System;
using Storage;

using UnityEngine;

class CreateArchery : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.ARCHERY; } }

    public CreateArchery(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
