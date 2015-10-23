using UnityEngine;
using System.Collections;

public class AIDebugSystem : MonoBehaviour {

	void Start () {
	}
	
	void Update () {
	
	}

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "Hello Debug!");
    }
}
