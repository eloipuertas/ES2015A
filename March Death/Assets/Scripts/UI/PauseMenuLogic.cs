using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseMenuLogic : MonoBehaviour
{
	
	// Use this for initialization
	void Start ()
	{
		GameObject.Find ("Resume").GetComponent<Button> ().onClick.AddListener (() => {
			QuitPauseMenu (); });
		GameObject.Find ("Exit").GetComponent<Button> ().onClick.AddListener (() => {
			QuitToMainMenu (); });
	}

	/// <summary>
	/// This method quits the game and goes back to main menu.
	/// </summary>
	void QuitToMainMenu ()
	{
		MenuButtonLogic.Pause_Play ();
		Application.LoadLevel (0);
	}

	/// <summary>
	/// This method destroys the pausemenu panel.
	/// </summary>
	void QuitPauseMenu ()
	{
        GameObject root = GameObject.Find ("BackgroundMenuPanel");
		Destroy (root);

		MenuButtonLogic.Pause_Play ();
	}

}
