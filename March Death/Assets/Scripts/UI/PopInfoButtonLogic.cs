using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopInfoButtonLogic : MonoBehaviour {

	// Use this for initialization
	void Start () {

        gameObject.GetComponent<Button>().onClick.AddListener(() => 
            {
                toggle();
            });


        GameObject.Find("HUD/populationInfo").GetComponent<Image>().enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void toggle()
    {
        GameObject.Find("HUD/populationInfo").GetComponent<Image>().enabled ^= true;
        HUDPopulationInfo.ToggleVision();
    }
}
