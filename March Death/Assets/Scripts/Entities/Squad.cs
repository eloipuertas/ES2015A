using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;

public sealed class Squad : BareObserver<Squad.Actions>
{
    public enum Actions { UNIT_ADDED, UNIT_REMOVED }

    public object UserData = null;

    #region Private Attributes
    private Dictionary<DataType, ISquadData> _data = new Dictionary<DataType, ISquadData>();
    private List<Unit> _units = new List<Unit>();

    private AutoUnregister _auto;
    #endregion

    #region Public Getters/Setters
    public List<Unit> Units
    {
        get
        {
            return _units.ToList();
        }
    }

    // TODO: Precach all
    public List<Selectable> Selectables
    {
        get
        {
            return _units.Select(x => x.GetComponent<Selectable>()).ToList();
        }
    }

    public BoundingBox.BoundingBoxHolder BoundingBox
    {
        get
        {
            return ((BoundingBox)_data[DataType.BOUNDING_BOX]).Value;
        }
    }

    public float Attack
    {
        get
        {
            return ((AttackValue)_data[DataType.ATTACK_VALUE]).Value;
        }
    }

    public Squad EnemySquad
    {
        get
        {
            return ((SmelledEnemies)_data[DataType.SMELLED_ENEMIES]).Value;
        }
    }

    private Storage.Races _race;
    public Storage.Races Race
    {
        get
        {
            return _race;
        }
    }

    // FIXME: Use the squad race :/
    private float _maxAttackRange;
    public float MaxAttackRange
    {
        get
        {
            return _maxAttackRange;
        }
    }
    #endregion

    public Squad(Storage.Races race, bool smellEnemies = true)
    {
        _auto = new AutoUnregister(this);
        _race = race;

        _data.Add(DataType.BOUNDING_BOX, new BoundingBox(this));
        _data.Add(DataType.ATTACK_VALUE, new AttackValue(this));

        // Smelled squads can not smell enemies or we would run into endless recursion
        if (smellEnemies)
        {
            _maxAttackRange = Storage.Info.get.of(_race, Storage.UnitTypes.THROWN).unitAttributes.rangedAttackFurthest;
            _data.Add(DataType.SMELLED_ENEMIES, new SmelledEnemies(this));
        }
        else
        {
            _maxAttackRange = 1f;
        }
    }

    private void OnUnitDied(System.Object obj)
    {
        RemoveUnit(((GameObject)obj).GetComponent<Unit>());
    }

    public void AddUnit(Unit unit)
    {
        if (!_units.Contains(unit))
        {
            _units.Add(unit);
            unit.Squad = this;
            _auto += unit.register(Unit.Actions.DIED, OnUnitDied);

            fire(Actions.UNIT_ADDED, unit);
        }
    }

    public void AddUnits(IEnumerable<Unit> units)
    {
        foreach (Unit unit in units)
        {
            AddUnit(unit);
        }
    }

    public void AddUnits(Squad squad)
    {
        AddUnits(squad.Units);
    }

    public void RemoveUnit(Unit unit)
    {
        _units.Remove(unit);
        _auto -= unit.unregister(Unit.Actions.DIED, OnUnitDied);

        fire(Actions.UNIT_REMOVED, unit);
    }

    public void UpdateUnits(IEnumerable<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (!_units.Contains(unit))
            {
                AddUnit(unit);
            }
        }

        foreach (Unit unit in _units.ToList())
        {
            if (!units.Contains(unit))
            {
                RemoveUnit(unit);
            }
        }
    }

    public void MoveTo(Vector3 target, Action<Unit> callback = null)
    {
        foreach (Unit unit in _units)
        {
            unit.moveTo(target);

            if (callback != null)
            {
                callback(unit);
            }
        }
    }

    public void AttackTo(IGameEntity target, Action<Unit> callback = null)
    {
        foreach (Unit unit in _units)
        {
            unit.attackTarget(target);

            if (callback != null)
            {
                callback(unit);
            }
        }
    }

    public void EnterTo(IGameEntity target, Action<Unit> callback = null)
    {
        foreach (Unit unit in _units)
        {
            if (unit.info.isCivil)
            {
                unit.goToBuilding(target);

                if (callback != null)
                {
                    callback(unit);
                }
            }
        }
    }

    public void Update()
    {
        foreach (var entry in _data)
        {
            // TODO: Smarter BoundingBox update, only when moving
            if (entry.Key == DataType.BOUNDING_BOX)
            {
                entry.Value.ForceUpdate();
            }

            entry.Value.Update(_units);
        }
    }

    public override void OnDestroy()
    {
        foreach (var entry in _data)
        {
            entry.Value.OnDestroy();
        }

        base.OnDestroy();
    }
}

public sealed class SquadUpdater : GameEntity<SquadUpdater.DummyActions>
{
    public enum DummyActions { }

    private EntityStatus _defaultStatus = EntityStatus.IDLE;
    public override EntityStatus DefaultStatus
    {
        get
        {
            return _defaultStatus;
        }
        set
        {
            _defaultStatus = value;
        }
    }

    #region Not Implemented Overrides
    public override E getType<E>() { throw new NotImplementedException(); }

    public override IKeyGetter registerFatalWounds(Action<object> func)
    {
        throw new NotImplementedException();
    }

    public override IKeyGetter unregisterFatalWounds(Action<object> func)
    {
        throw new NotImplementedException();
    }

    protected override void onFatalWounds()
    {
        throw new NotImplementedException();
    }

    protected override void onReceiveDamage()
    {
        throw new NotImplementedException();
    }
    #endregion

    private Squad _squad;
    public Squad UnitsSquad { get { return _squad; } }
    
    public void Initialize(Storage.Races race)
    {
        _squad = new Squad(race);
    }

    public override void Awake() { }
    public override void Start() { }

    public override void Update()
    {
        base.Update();
        _squad.Update();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        _squad.OnDestroy();
    }
}

public enum DataType { BOUNDING_BOX, ATTACK_VALUE, SMELLED_ENEMIES }

public interface ISquadData
{
    bool NeedsUpdate { get; }
    void Update(List<Unit> units);
    void ForceUpdate();
    void OnDestroy();
}

public abstract class SquadData<T> : BareObserver<SquadData<T>.Actions>, ISquadData
{
    public enum Actions { }

    public abstract T Value { get; }

    protected bool _needsUpdate = true;
    public bool NeedsUpdate { get { return _needsUpdate; } }
    public abstract void Update(List<Unit> units);

    private AutoUnregister auto;
    public SquadData(Squad squad)
    {
        auto = new AutoUnregister(this);
        auto += squad.register(Squad.Actions.UNIT_ADDED, OnAdded);
        auto += squad.register(Squad.Actions.UNIT_REMOVED, OnRemoved);
    }

    public void ForceUpdate() { _needsUpdate = true; }
    public virtual void OnAdded(System.Object obj) { _needsUpdate = true; }
    public virtual void OnRemoved(System.Object obj) { _needsUpdate = true; }
}

public class BoundingBox : SquadData<BoundingBox.BoundingBoxHolder>
{
    public class BoundingBoxHolder
    {
        public Rect Bounds;
        public float MaxLongitude;
    }

    private BoundingBoxHolder _boundingBox = new BoundingBoxHolder();
    public override BoundingBoxHolder Value { get { return _boundingBox; } }

    public BoundingBox(Squad squad) : base(squad)
    {
    }

    public static Rect CalculateOf(List<Unit> units)
    {
        float minX = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxY = -Mathf.Infinity;

        foreach (Unit unit in units)
        {
            Vector3 position = unit.transform.position;

            if (maxY < position.z) maxY = position.z;
            if (minY > position.z) minY = position.z;
            if (maxX < position.x) maxX = position.x;
            if (minX > position.x) minX = position.x;
        }

        return new Rect(minX, minY, (maxX - minX) * 2, (maxY - minY) * 2);
    }

    public override void Update(List<Unit> units)
    {
        if (_needsUpdate)
        {
            Rect boundingBox = CalculateOf(units);
            _boundingBox.Bounds = boundingBox;
            _boundingBox.MaxLongitude = boundingBox.width > boundingBox.height ? boundingBox.width : boundingBox.height;
            _boundingBox.MaxLongitude = _boundingBox.MaxLongitude < 1f ? 1f : _boundingBox.MaxLongitude;

            _needsUpdate = false;
        }
    }
}

public class AttackValue : SquadData<float>
{
    private float _attack;
    public override float Value { get { return _attack; } }

    public AttackValue(Squad squad) : base(squad)
    {
    }

    public static float OfUnit (Unit unit)
    {
        return unit.healthPercentage / 100 * (unit.info.unitAttributes.resistance + unit.info.unitAttributes.attackRate * unit.info.unitAttributes.strength);
    }

    public override void Update(List<Unit> units)
    {
        if (_needsUpdate)
        {
            _attack = 0;
            foreach (Unit unit in units)
            {
                _attack += OfUnit(unit);
            }

            _needsUpdate = false;
        }
    }
}

public class SmelledEnemies : SquadData<Squad>
{
    private Squad _ownSquad;
    private Squad _enemySquad = new Squad(Storage.Races.RESERVED_UNSPECIFIED, false);
    public override Squad Value { get { return _enemySquad; } }

    public SmelledEnemies(Squad squad) : base(squad)
    {
        _ownSquad = squad;
    }
    
    public override void Update(List<Unit> units)
    {
        if (units.Count > 0)
        {
            BoundingBox.BoundingBoxHolder boundingBox = _ownSquad.BoundingBox;

#if UNITY_EDITOR
            Debug.DrawLine(new Vector3(boundingBox.Bounds.x, units[0].transform.position.y, boundingBox.Bounds.y), new Vector3(boundingBox.Bounds.x + boundingBox.MaxLongitude, units[0].transform.position.y, boundingBox.Bounds.y + boundingBox.MaxLongitude), Color.white);
#endif

            List<Unit> enemyUnits = AISenses.getVisibleUnitsNotOfRaceNearPosition(new Vector3(boundingBox.Bounds.x, units[0].transform.position.y, boundingBox.Bounds.y), boundingBox.MaxLongitude * 3 * _ownSquad.MaxAttackRange, _ownSquad.Race);
            _enemySquad.UpdateUnits(enemyUnits);
            _enemySquad.Update();
        }
    }
}
