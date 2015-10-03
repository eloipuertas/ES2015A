using UnityEngine;
using System.Collections;

public class Main_Game : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Instantiate((GameObject)Resources.Load ("HUD")).name = "HUD";
		Instantiate((GameObject)Resources.Load ("HUD_EventSystem")).name = "HUD_EventSystem";
    }

}
