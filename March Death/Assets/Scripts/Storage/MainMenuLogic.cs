using UnityEngine;
using System.Collections;

public class MenuLogic : MonoBehaviour {

	/* Each one of these bool variables corresponds to the 
	 * options of the main menu. */
	//public bool bStart = false;
	//public bool bTutorial = false;
	//public bool bQuit = false;  // these variables wont be needed if we use tags

	const Color UP_CLICK = new Color(1.0, 1.0, 0.0, 0.6);
	const Color DOWN_CLICK = new Color(0.0, 0.0, 0.0, 0.6);

	// Use this for initialization
	void Start () {
		Cursor.visible = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/* This method changes the color of the object we are clicking */
	void OnMouseDown() {
		GetComponent<Renderer>().material.color = UP_CLICK;
	}

	/* This method moves to another scene or quit */
	void OnMouseUp() {

		GetComponent<Renderer>().material.color = DOWN_CLICK;

		if(this.CompareTag("bStart")) { Application.LoadLevel(1); }
		else if(this.CompareTag("bTutorial")) { Application.LoadLevel(2); }
		else { Application.Quit(); }
	}
}
