using UnityEngine;
using System.Collections;
using System;

public class Building : MonoBehaviour, IGameEntity
{
    public string fakeType;
    public float fakeHealthRatio;


    public string FakeType
    {
        get
        {
            return fakeType;
        }
    }

    public float FakeHealthRatio
    {
        get
        {
            return fakeHealthRatio;
        }
    }

    // Use this for initialization
    void Start() { }

    // Update is called once per frame
    void Update() { }


}