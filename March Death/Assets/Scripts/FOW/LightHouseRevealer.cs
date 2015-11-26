using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class LightHouseRevealer : MonoBehaviour
{


    public enum Direction { CLOCK, COUNTERCLOCK };
    public Direction direction = Direction.CLOCK;
    private int Angle = 360;
    public float RevealerStep = 1f;
    public float RevealerSpeed = 1f;
    
    private float ApertureStep = .25f;
    private int RevealerRange = 55;

    private Vector3 _basePosition;
    private float _endOpening;
    private bool _opening;
    public bool _rotating;
    private LightHouse _lightHouse;
    private Vector3 _center;

    void Awake()
    {
        _basePosition = transform.position;
        _endOpening = _basePosition.z + RevealerRange;
        _opening = true;
    }

    void Start()
    {
        Debug.Log("Lighthouse opening");
        _lightHouse = transform.parent.gameObject.GetComponent<LightHouse>();
        _center = transform.parent.position;
    }

    void Update()
    {
        if (_opening)
        {
            Vector3 newPos = transform.position;
            newPos.z += ApertureStep;
            transform.position = newPos;
            if (newPos.z > _endOpening)
            {
                Debug.Log("Lighthouse on position");
                _opening = false;
                _rotating = true;
            }

        }
        else if(_rotating)
        {
            transform.RotateAround(_center, Vector3.up, RevealerStep);
        }

    }



}

