using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


class SmartGate : MonoBehaviour
{



    private float _radius = 20;
    private Animator _animator;
    private SphereCollider _collider;
    private List<IGameEntity> _enemies = new List<IGameEntity>();
    private List<IGameEntity> _allies = new List<IGameEntity>();
    private DetourFlag _door;
    private Storage.Races _race;
    private Status _status;

    public enum Status {
        OPEN,       // gates open and animator showing gates open
        CLOSE,      // gates closed and animator showing gates closed
        ALWAYS_OPEN,// gates always open doesn't matter if there are enemies or not
        ALWAYS_CLOSE// doors always closed doesn't matter if there are allies or not
    };



    public void Start()
    {
        _race = transform.parent.GetComponent<Barrack>().getRace();
        _collider = GetComponent<SphereCollider>();
        _collider.radius = 0;
        _status = Status.CLOSE;
        _animator = transform.parent.GetComponent<Animator>();
        transform.parent.GetComponent<Barrack>().register(Barrack.Actions.BUILDING_FINISHED, OnBuildingFinished);
        _door = transform.parent.GetComponent<DetourFlag>();



    }

    public void OnBuildingFinished(object obj)
    {
        _collider.radius = _radius;
        CheckStatus();
    }


    public void Update()
    {


    }

    void OnTriggerEnter(Collider col)
    {

        GameObject obj = col.gameObject;
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        if (entity != null)
        {
            if (IsAllie(entity))
            {
                if (!_allies.Contains(entity))
                    AcceptAllie(entity);
            }
            else if (IsEnemy(entity))
            {

                if (!_enemies.Contains(entity))
                    AcceptEnemy(entity);
            }
        }
    }


    void OnTriggerExit(Collider col)
    {
        GameObject obj = col.gameObject;
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        if (entity != null)
        {
            if (IsAllie(entity))
            {
                if (_allies.Contains(entity))
                    RemoveAllie(entity);
            }
            else if (IsEnemy(entity))
            {

                if (_enemies.Contains(entity))
                    RemoveEnemy(entity);
            }
        }
    }


    void AcceptAllie(IGameEntity entity)
    {
        _allies.Add(entity);
        CheckStatus();
    }


    void RemoveAllie(IGameEntity entity)
    {
        _allies.Remove(entity);
        CheckStatus();
    }

    void AcceptEnemy(IGameEntity entity)
    {
        _enemies.Add(entity);
        CheckStatus();
    }

    void RemoveEnemy(IGameEntity entity)
    {
        _enemies.Remove(entity);
        CheckStatus();
    }

    private bool IsAllie(IGameEntity entity)
    {
        return entity.info.isUnit && entity.info.race == _race;
    }

    private bool IsEnemy(IGameEntity entity)
    {
        return entity.info.isUnit && entity.info.race != _race;
    }

    public void CheckStatus()
    {

        if (_status == Status.ALWAYS_CLOSE && _status == Status.ALWAYS_OPEN) return;

        // if there are allies around,the gates will remain open
        if (_allies.Count > 0) SetStatus(Status.OPEN);
        // if there are no allies but there are enemies, the gates will remain closed
        //else if (_enemies.Count > 0) SetStatus(Status.CLOSE);-----------------------------
        // if there aren't allies neither enemies, the gates will remain idle
        else SetStatus(Status.CLOSE);

    }

    public void SetStatus(Status status)
    {
        _status = status;

        switch(_status)
        {

            case Status.OPEN:
            case Status.ALWAYS_OPEN:
                OpenGate(true);
                break;
            case Status.CLOSE:
            case Status.ALWAYS_CLOSE:
                OpenGate(false);
                break;
        }
    }

    public void OpenGate(bool _open)
    {
        if (_open)
        {

            _door.enabled = !_open;
            _animator.SetBool("open", _open);
            Debug.Log("Gate is open");
        }
        else
        {
            _door.enabled = _open;
            _animator.SetBool("open", _open);
            Debug.Log("Gate is closed");
        }

    }
}

