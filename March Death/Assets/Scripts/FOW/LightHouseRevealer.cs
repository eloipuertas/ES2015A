using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class LightHouseRevealer : MonoBehaviour
{

    private Storage.Races _race;
    public enum Status { STOP, FOCUSED, IDLE };
    public enum Direction { CLOCK, COUNTERCLOCK };
    public Direction direction = Direction.CLOCK;
    public Status status = Status.STOP;

    private float _revealerAngleStep = 1f;
    private float _focusingStep = 5f;
    private float _revealerOrbitatingRadius = 50;

    private Vector3 _referencePosition;

    private GameObject _light;
    private FOWEntity _revealer;
    private GameObject _target;


    private Unit _attacker;


    private float _offset = 1f;

    public virtual void Start()
    {
        //moves the revealer to the orbitating position

        transform.position = transform.parent.position;
        _race = transform.parent.GetComponent<Barrack>().getRace();
        _attacker = transform.parent.GetComponent<Unit>();

        _revealer = GetComponent<FOWEntity>();
        _revealer.Activate(_race);

        _light = transform.parent.FindChild("LightHouse-Detector").gameObject;
        _light.SetActive(false);

        transform.parent.GetComponent<Barrack>().register(Barrack.Actions.BUILDING_FINISHED, OnBuildingFinished);
        _target = null;

    }



    public virtual void Update()
    {
        if (status == Status.IDLE)
        {
            Vector3 rotateDirection = direction == Direction.CLOCK ? Vector3.up : Vector3.down;
            transform.RotateAround(_referencePosition, rotateDirection, _revealerAngleStep);
        }

        else if (status == Status.FOCUSED && _target != null)
        {

            if (EnoughDifference(transform.position, _target.transform.position))
            {
                transform.position = GetPosition(_target.transform.position);
                //transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, _focusingStep);

            }
        }
        else if (status != Status.STOP)
        {
            Debug.Log("Trying to acces to a null target");
            ReturnOrbitatingDistance();
            SetStatus(Status.IDLE);
            _target = null;
        }

        _light.transform.LookAt(transform);

       DrawInfo();

    }

    /// <summary>
    /// Enables the lighthouse system only when the building is finished
    /// </summary>
    /// <param name="obj"></param>
    public void OnBuildingFinished(object obj)
    {
        _referencePosition = transform.parent.position;
        transform.Translate(new Vector3(_revealerOrbitatingRadius, 0f, 0f));
        _light.SetActive(true);
        SetStatus(Status.IDLE);
    }


    private Vector3 GetPosition(Vector3 target)
    {
        float distance = Vector3.Distance(_referencePosition, target);

        if (distance > _revealerOrbitatingRadius)// the target is beyond the radius
        {
            //Vector3 direction = (target - _referencePosition);
            //Vector3 farPoint = (direction / distance) * _revealerOrbitatingRadius;

            Vector3 direction = (target - _referencePosition).normalized;
            Vector3 farPoint = direction * _revealerOrbitatingRadius;

            return _referencePosition + farPoint;

        }

        return target;
    }

    

    private void ReturnOrbitatingDistance()
    {

        float distance = Vector3.Distance(_referencePosition, transform.position);
        Vector3 direction, movement, farPoint;

        if (distance < _revealerOrbitatingRadius)// the target is closer than the radius
        {
            direction = (transform.position - _referencePosition ).normalized;
            movement = direction * _revealerOrbitatingRadius;
            farPoint = _referencePosition + movement;

            //haaaaaaack
            transform.position = farPoint;
            //Vector3.MoveTowards(transform.position, farPoint, 0f);
        }

    }

    /// <summary>
    /// Toggles the orbitating direction
    /// </summary>
    public void ToggleDirection()
    {
        direction = direction == Direction.CLOCK ? Direction.COUNTERCLOCK : Direction.CLOCK;
        Debug.Log("Now rotating " + direction);
    }


    /// <summary>
    /// Checks if two vectors are different by applying an offset
    /// </summary>
    /// <param name="now"></param>
    /// <param name="then"></param>
    /// <returns></returns>
    private bool EnoughDifference(Vector3 now, Vector3 then)
    {
        //Debug.Log(Vector3.Distance(now, then) + " " + _offset);
        return Vector3.Distance(now, then) > _offset;
    }


    public void SetStatus(Status status)
    {
        Debug.Log("Watchtower status: " + status);

        this.status = status;
    }

    public void FocusTarget(IGameEntity entity)
    {
        SetStatus(Status.FOCUSED);
        _target = entity.getGameObject();
        _attacker.attackTarget(entity);

    }

    public void StopFocusing()
    {
        SetStatus(Status.IDLE);
        _attacker.stopAttack();
        ReturnOrbitatingDistance();
        _target = null;
     }


    #region INFO

#if  UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.parent.position, Vector3.down, _revealerOrbitatingRadius);
    }
#endif
    private void DrawInfo()
    {
        if (_target)
        {
            Debug.DrawLine(transform.position, _target.transform.position, Color.black);
        }
    }
    #endregion
}

