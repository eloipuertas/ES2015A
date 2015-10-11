using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;


/// <summary>
/// Unit base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Unit : GameEntity<Unit.Actions>
{
    public enum Actions { MOVEMENT_START, MOVEMENT_END, DAMAGED, DIED };

    public Unit() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public Races race = Races.MEN;
    public UnitTypes type = UnitTypes.HERO;

    /// <summary>
    /// If in battle, this is the target and last attack time
    /// </summary>
    private Unit _target = null;
    private double _lastAttack = 0;

    /// <summary>
    /// Point to move to
    /// </summary>
    private Vector3 _movePoint;

    /// <summary>
    /// Civil units might need this to acount how many *items* they are carrying.
    /// </summary>
    public int usedCapacity { get; set; }

    /// <summary>
    /// Called once our target dies. It may be used to update unit IA
    /// </summary>
    /// <param name="gob"></param>
    private void onTargetDied(GameObject gob)
    {
        // TODO: Our target died, select next? Do nothing?
        _status = EntityStatus.IDLE;
    }

    /// <summary>
    /// When a wound is received, this is called
    /// </summary>
    protected override void onReceiveDamage()
    {
        fire(Actions.DAMAGED);
    }

    /// <summary>
    /// When wounds reach its maximum, thus unit dies, this is called
    /// </summary>
    protected override void onFatalWounds()
    {
        fire(Actions.DIED);
    }

    /// <summary>
    /// Sets up our attack target, registers callback for its death and
    /// updates our state
    /// </summary>
    /// <param name="unit"></param>
    public void attackTarget(Unit unit)
    {
        _target = unit;
        _target.register(Actions.DIED, onTargetDied);
        _status = EntityStatus.ATTACKING;
    }

    /// <summary>
    /// Stops attacking the target and goes back to an IDLE state
    /// </summary>
    public void stopAttack()
    {
        _target.unregister(Actions.DIED, onTargetDied);
        // TODO: Maybe we should not set it to null? In case we want to attack it again
        _target = null;
        _status = EntityStatus.IDLE;
    }

    /// <summary>
    /// Starts moving the unit towards a point on a terrain
    /// </summary>
    /// <param name="movePoint">Point to move to</param>
    public void moveTo(Vector3 movePoint)
    {
        _movePoint = movePoint;

        float distance = Vector3.Distance(movePoint, transform.position);
        if (distance > 1.50f)
        {
            // TODO: SMOOTH TURNING
            Quaternion targetRotation = Quaternion.LookRotation(movePoint - transform.position);
            targetRotation.x = 0;   // lock rotation on x-axis
            transform.rotation = targetRotation;
        }

        _status = EntityStatus.MOVING;
        fire(Actions.MOVEMENT_START);
    }
    
    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Start()
    {
        _status = EntityStatus.IDLE;
        _info = Info.get.of(race, type);

        // Call GameEntity start
        base.Start();
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    public override void Update()
    {
        base.Update();

#if TEST_INPUT
        if (Input.GetMouseButtonDown(0))
        {
            Camera mainCamera = Camera.main;

            // We need to actually hit an object
            RaycastHit hit;
            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 1000))
                return;

            // We need to hit something (with a collider on it)
            if (!hit.transform)
                return;

            // Get input vector from kayboard or analog stick and make it length 1 at most
            moveTo(hit.point);
        }
#endif
        switch (_status)
        {
            case EntityStatus.ATTACKING:
                if (_target != null)
                {
                    if (Time.time - _lastAttack >= (1f / info.unitAttributes.attackRate))
                    {
                        // TODO: Ranged attacks!
                        _target.receiveAttack(this, false);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
        switch (_status)
        {
            case EntityStatus.MOVING:
                // TODO: Steps on the last sector are smoothened due to distance being small
                // Althought it's an unintended behaviour, it may be interesating to leave it as is
                transform.position = Vector3.MoveTowards(transform.position, _movePoint, Time.fixedDeltaTime * info.unitAttributes.movementRate);

                // If distance is lower than 0.5, stop movement
                if (Vector3.Distance(transform.position, _movePoint) <= 0.5f)
                {
                    _status = EntityStatus.IDLE;
                    fire(Actions.MOVEMENT_END);
                }
                break;
        }
    }

}
