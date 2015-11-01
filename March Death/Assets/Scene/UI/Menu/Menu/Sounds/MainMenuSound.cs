using UnityEngine;
using System.Collections;

public class MainMenuSound : MonoBehaviour { 


	static MainMenuSound instance;
	public static MainMenuSound GetInstance(){
		return instance;
	}

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