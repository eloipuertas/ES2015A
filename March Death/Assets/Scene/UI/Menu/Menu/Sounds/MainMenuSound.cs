using UnityEngine;
using System.Collections;
#pragma strict
public class MainMenuSound : MonoBehaviour { 
	/*
	AudioSource audio;

	void Awake() {
		// see if we've got game music still playing
		audio = GetComponent<AudioSource>();
		GameObject menuMusic = GameObject.Find("BackgroundMusic");

		if (!audio.isPlaying) {
			audio.Play ();
		} else {
			// kill game music
			audio.Stop();
			Destroy(menuMusic);
		}
		// make sure we survive going to different scenes
		DontDestroyOnLoad(gameObject);
	}
*/
	MainMenuSound instance;
	void Awake() 
	{
		if ( instance != null && instance != this ) 
		{
			Destroy( this.gameObject );
			return;
		} 
		else 
		{
			instance = this;
		}
		
		DontDestroyOnLoad( this.gameObject );
	}
}