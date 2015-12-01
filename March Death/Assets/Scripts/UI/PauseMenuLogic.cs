using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseMenuLogic : MonoBehaviour
{
    Main_Game mg;
    public static bool isPauseMenuShown;
	
	// Use this for initialization
	void Start ()
	{
        mg = GameObject.FindWithTag("GameController").GetComponent<Main_Game>();
		GameObject.Find ("Resume").GetComponent<Button> ().onClick.AddListener (() => {
			QuitPauseMenu (); });
		GameObject.Find ("Exit").GetComponent<Button> ().onClick.AddListener (() => {
			QuitToMainMenu (); });

        isPauseMenuShown = false;
	}

	/// <summary>
	/// This method quits the game and goes back to main menu.
	/// </summary>
	void QuitToMainMenu ()
	{
		MenuButtonLogic.Pause_Play ();
        mg.ClearGame();
        Application.LoadLevel (0);
	}

	/// <summary>
	/// This method destroys the pausemenu panel.
	/// </summary>
	static void QuitPauseMenu ()
	{
        GameObject root = GameObject.Find ("PausePanel");
		Destroy (root);

		MenuButtonLogic.Pause_Play ();
	}

    public static void TogglePauseMenu()
    {
        if (MenuButtonLogic.bPaused)
        {
            QuitPauseMenu();
        }
        else
        {
            MenuButtonLogic.Pause_Play();
        }
    }

}
