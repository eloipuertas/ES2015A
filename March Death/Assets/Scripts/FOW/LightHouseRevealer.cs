using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class LightHouseRevealer : MonoBehaviour
{

    private Storage.Races _race;
    public enum Direction { CLOCK, COUNTERCLOCK };
    public Direction direction = Direction.CLOCK;

    private int Angle = 360;
    private float RevealerAngleStep = 2f;
    private float ApertureStep = 1f;
    private int RevealerApertureRange = 60;

    private Vector3 _basePosition;
    private int _endOpening;
    private bool _opening;
    private bool _rotating;
    private Vector3 _center;
    private GameObject light;
    

    void Awake()
    {
        _basePosition = transform.position;
        _endOpening = RevealerApertureRange;
        _opening = true;
    }

    void Start()
    {
        Debug.Log("Lighthouse opening");
        _center = transform.parent.position;
        _race = transform.parent.GetComponent<Barrack>().getRace();
        GetComponent<FOWEntity>().Activate(_race);
        _opening = true;
        light = transform.parent.FindChild("Light").gameObject;

    }



    void Update()
    {
        if (_opening)
        {
            Vector3 newPos = transform.position;
            newPos.z += ApertureStep;
            transform.position = newPos;
            if (--_endOpening < 0)
            {
                Debug.Log("Lighthouse on position");
                _rotating = true;
                _opening = false;
            }

        }
        else if(_rotating)
        {
            Vector3 rotateDirection = direction == Direction.CLOCK ? Vector3.up : Vector3.down;
            transform.RotateAround(_center, rotateDirection, RevealerAngleStep);
        }

        light.transform.LookAt(transform);
    }

    public void ToggleDirection()
    {
        direction = direction == Direction.CLOCK ? Direction.COUNTERCLOCK : Direction.CLOCK;
        Debug.Log("Now rotating " + direction);
    }


}

