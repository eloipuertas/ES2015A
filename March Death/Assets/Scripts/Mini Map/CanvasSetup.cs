using UnityEngine;
using System.Collections;

public class CanvasSetup : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.gameObject.GetComponent<Canvas>().worldCamera = GameObject.Find("UI Camera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
