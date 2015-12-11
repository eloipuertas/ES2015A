using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour {
    private Vector3 _end_point;
    private Unit _owner;
    private int _speed;
    private float _radius;

	// Use this for initialization
	void Start ()
    {
	}

    void Awake()
    {
    }

    public void setProps(Vector3 end_point, Unit owner, int speed, float radius)
    {
        _owner = owner;
        _end_point = end_point;
        _radius = radius;
        _speed = speed;
    }

    // Update is called once per frame
    void Update ()
    {
        //Find a new position proportionally closer to the end, based on the projectileSpeed
        Vector3 newPostion = Vector3.MoveTowards(gameObject.transform.position, _end_point, _speed * Time.deltaTime);

        //Move the object to the new position.
        gameObject.transform.position = newPostion;

        //Recalculate the remaining distance after moving.
        float sqrRemainingDistance = (gameObject.transform.position - _end_point).sqrMagnitude;

        // If we reach the target...
        if (sqrRemainingDistance <= float.Epsilon)
        {
            List<IGameEntity> objectsInRadius = Helpers.getEntitiesNearPosition(_end_point, _radius);
            
            // Should I prevent friendly fire?
            foreach (IGameEntity inRadiusObject in objectsInRadius.ToArray())
            {
                if (inRadiusObject.status != EntityStatus.DEAD)
                {
                    inRadiusObject.receiveAttack(_owner, _owner.canDoRangedAttack());
                }
            }
            
            Destroy(gameObject);
        }
    }
}
