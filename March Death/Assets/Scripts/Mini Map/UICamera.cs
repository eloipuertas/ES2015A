using UnityEngine;
using System.Collections;

public class UICamera : MonoBehaviour {

    private Camera _uiCam;
    private Camera _minCam;

    public int depth;

	void Start ()
    {
        depth = 20;
        _minCam = GameObject.Find("Minimap Camera").GetComponent<Camera>();
        _uiCam = gameObject.GetComponent<Camera>();

	}
	
	void Update () {

    }

    void OnGUI() {}
}
