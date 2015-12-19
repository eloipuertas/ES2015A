using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class LightHouseDetector : MonoBehaviour
{
    private float _radius = 80;
    private float _scaleOffset = 2f;
    private SphereCollider _collider;
    private LightHouseRevealer _revealer;
    private List<IGameEntity> _targets = new List<IGameEntity>();
    private Storage.Races _race;
    private GameObject _currentTarget = null;

    public void Start()
    {
        _race = transform.parent.GetComponent<Barrack>().getRace();
        _revealer = transform.parent.FindChild("LightHouse-Revealer").GetComponent<LightHouseRevealer>();
        _collider = GetComponent<SphereCollider>();
        _collider.radius = _radius / _scaleOffset;
        _collider.center = new Vector3(0f, -8f, 0f);

    }

    public void Update()
    {
        DrawInfo();// <- buggy
    }

    /// <summary>
    /// Stops orbitating if it's enemy
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter(Collider col)
    {

        GameObject obj = col.gameObject;
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        if (entity != null)
        {
            if (IsTarget(entity) && !_targets.Contains(entity))
            {
                AcceptTarget(entity);
            }
        }
    }

    /// <summary>
    ///  Just start orbitating
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerExit(Collider col)
    {
        GameObject obj = col.gameObject;
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        if (entity != null)
        {
            if (IsTarget(entity) && _targets.Contains(entity))
            {
                RemoveTarget(entity);
            }
        }
    }

    /// <summary>
    /// Returns if its a target by checking if it's unity and a different race
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private bool IsTarget(IGameEntity entity)
    {
        return entity.info.isUnit && entity.info.race != _race && entity.status != EntityStatus.DEAD;
        //return entity.info.isUnit;
    }

    /// <summary>
    /// Event when a unit dies
    /// </summary>
    /// <param name="obj"></param>
    public void OnUnitDied(object obj)
    {
        IGameEntity entity = ((GameObject)obj).GetComponent<IGameEntity>();
        if (IsTarget(entity) && _targets.Contains(entity))
        {
            RemoveTarget(entity);
        }

    }



    /// <summary>
    /// Performs the actions when a target is accepted
    /// </summary>
    /// <param name="entity"></param>
    private void AcceptTarget(IGameEntity entity)
    {
        _targets.Add(entity);
        RegisterEvents(entity);

        if (_currentTarget == null)
        {
            _currentTarget = entity.getGameObject();
            _revealer.FocusTarget(entity);
        }
    }


    /// <summary>
    /// Performs the action to remove a target from the list
    /// </summary>
    /// <param name="entity"></param>
    private void RemoveTarget(IGameEntity entity)
    {
        _targets.Remove(entity);
        UnregisterEvents(entity);

        if (_currentTarget == entity.getGameObject())
        {
            if (_targets.Any())
            {
                _currentTarget = _targets[0].getGameObject();
                _revealer.FocusTarget(_currentTarget.GetComponent<IGameEntity>());
            }
            else
            {
                _currentTarget = null;
                _revealer.StopFocusing();
            }
        }
        Debug.Log("Remaining targets: " + _targets.Count);
    }


    /// <summary>
    /// Register events for a detected target
    /// </summary>
    /// <param name="entity"></param>
    public void RegisterEvents(IGameEntity entity)
    {
        Unit unit = (Unit)entity;
        unit.register(Unit.Actions.DIED, OnUnitDied);
    }

    /// <summary>
    /// Unregister events for a detected target
    /// </summary>
    /// <param name="entity"></param>
    public void UnregisterEvents(IGameEntity entity)
    {
        Unit unit = (Unit)entity;
        unit.unregister(Unit.Actions.DIED, OnUnitDied);
    }

    void OnDestroy()
    {
        if (_targets.Any())
        {
            foreach (IGameEntity entity in _targets)
            {
                Unit unit = (Unit)entity;
                unit.unregister(Unit.Actions.DIED, OnUnitDied);
            }
        }

    }

    #region INFO

#if UNITY_EDITOR 
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(transform.parent.position, Vector3.down, _radius);
    }

#endif

    private void DrawInfo()
    {
        if (_targets.Any())
        {
            foreach (IGameEntity entity in _targets)
            {
                if (entity.status != EntityStatus.DEAD)
                {
                    if (entity.getGameObject() == _currentTarget)
                        Debug.DrawLine(transform.position, entity.getGameObject().transform.position, Color.red);
                    else
                        Debug.DrawLine(transform.position, entity.getGameObject().transform.position, Color.yellow);
                }
            }
        }

    }

    #endregion INFO


}

