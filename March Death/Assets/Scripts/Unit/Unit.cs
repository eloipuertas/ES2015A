using UnityEngine;

/// <summary>
/// Unit base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Unit : Utils.Actor<Unit.Actions>
{
    public enum Actions { DIED }

    public Unit() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public Storage.Races race = Storage.Races.MEN;
    public Storage.Types type = Storage.Types.HERO;

    /// <summary>
    /// Contains all static information of the Unit.
    /// That means: max health, damage, defense, etc.
    /// </summary>
    public Storage.UnitInfo info { get; set; }

    /// <summary>
    /// Contains actual health of the Unit.
    /// </summary>
    private float _health;
    public float health
    {
        get
        {
            return _health;
        }
    }
    
    /// <summary>
    /// Contains percentual value of health (100-0%)
    /// </summary>
    public float healthPct
    {
        get
        {
            return health * 100f / info.attributes.health;
        }
    }

    /// <summary>
    /// Updates health based on damage received from a unit
    /// </summary>
    /// <param name="from">Unit which attacked</param>
    public void receiveDamage(Unit from)
    {
        // TODO: Write formulas for damage calculation
        _health -= from.info.attributes.attack;

        if (_health <= 0)
        {
            _health = 0;
            fire(Actions.DIED);
        }
    }

	// Use this for initialization
	void Start () {
        info = Storage.InfoGather.get.getUnitInfo(race, type);
	}

	// Update is called once per frame
	void Update () {

	}

}
