using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InformationController : MonoBehaviour {

	// Use this for initialization
	void Start () {

		GameObject hud = GameObject.Find ("HUD");	
		if (hud != null) {
			Transform information = hud.transform.FindChild ("Information");
			if (information != null) {
				Transform txtActorName = information.transform.FindChild("ActorName");
				Transform txtActorRace = information.transform.FindChild ("ActorRace");
				Transform txtActorHealth = information.transform.FindChild("ActorHealth");
				Transform txtActorImage = information.transform.FindChild ("ActorImage");
				Transform sliderActorHealth = information.transform.FindChild ("ActorHealthSlider");
				
				txtActorName.gameObject.GetComponent<Text>().enabled = false;
				txtActorRace.gameObject.GetComponent<Text>().enabled = false;
				txtActorHealth.gameObject.GetComponent<Text>().enabled = false;
				txtActorImage.gameObject.GetComponent<Image>().enabled = false;
				sliderActorHealth.gameObject.GetComponent<Slider>().enabled = false;
				sliderActorHealth.gameObject.GetComponent<Slider>().value = 0;
				
				Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
				sliderBackground.GetComponent<Image>().enabled = false;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
