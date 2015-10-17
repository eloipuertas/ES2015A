using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuButtonLogic : MonoBehaviour {

	static bool bPaused = false;


	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Button>().onClick.AddListener( () => { Pause_Play (); } );
	}

    /// <summary>
    /// This method pauses / plays the game.
    /// </summary>
	public static void Pause_Play(){

		bPaused = !bPaused;

		if(bPaused)
			Instantiate((GameObject)Resources.Load ("PauseMenu")).name = "PausePanel"; 
	
		Time.timeScale = bPaused ? 0 : 1;
		GameObject.Find ("Button_Menu").GetComponent<Button>().interactable = !bPaused;
	}

}
