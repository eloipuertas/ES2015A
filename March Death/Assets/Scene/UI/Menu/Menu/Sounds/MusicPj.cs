using UnityEngine;
using System.Collections;

public class MusicPj : MonoBehaviour {

	void Awake() 
	{
		MainMenuSound mainMenu;
		mainMenu = MainMenuSound.GetInstance ();
		GameObject menuMusic = GameObject.Find("MusicPjBackground");
		menuMusic = mainMenu.gameObject;

	}
}
