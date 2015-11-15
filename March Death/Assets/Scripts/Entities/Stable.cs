using UnityEngine;
using System.Collections;


public class Stable : Building<Stable.Actions>
{

    public enum Actions { DAMAGED, DESTROYED, CREATE_UNIT };
    // Constructor
    public Stable() { }
    // Use this for initialization
    void Start ()
    {
        base.Start();
	}
	
	// Update is called once per frame
	void Update ()
    {
        base.Update();
	}
}
