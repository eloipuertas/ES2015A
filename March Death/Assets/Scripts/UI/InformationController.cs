using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Storage;

public class InformationController : MonoBehaviour {

	private Text txtActorName;
	private Text txtActorRace;
	private Text txtActorHealth;
	private Image imgActorDetail;
	private Slider sliderActorHealth;
	
	// Use this for initialization
	void Start () {

		//Init menu components
		Transform information = GameObject.Find ("HUD").transform.FindChild ("Information");
		txtActorName = information.transform.FindChild("ActorName").gameObject.GetComponent<Text>();
		txtActorRace = information.transform.FindChild ("ActorRace").gameObject.GetComponent<Text>();
		txtActorHealth = information.transform.FindChild("ActorHealth").gameObject.GetComponent<Text>();
		imgActorDetail = information.transform.FindChild ("ActorImage").gameObject.GetComponent<Image>();
		sliderActorHealth = information.transform.FindChild ("ActorHealthSlider").gameObject.GetComponent<Slider>();

		//Default is hidden
		hideInformation ();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	private void dosomething() 
	{
		IGameEntity entity = gameObject.GetComponent<IGameEntity>();
		Unit unit = entity.toUnit ();
		unit.register(Unit.Actions.DAMAGED, onUnitDamaged);
		unit.register(Unit.Actions.DAMAGED, onUnitDied);
	}

	private void hideInformation() 
	{	
		txtActorName.enabled = false;
		txtActorRace.enabled = false;
		txtActorHealth.enabled = false;
		imgActorDetail.enabled = false;
		sliderActorHealth.enabled = false;
		sliderActorHealth.value = 0;
		
		Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
		sliderBackground.GetComponent<Image>().enabled = false;

	}

	public void onUnitDamaged(GameObject gameObject)
	{
		IGameEntity entity = gameObject.GetComponent<IGameEntity> ();
		sliderActorHealth.value = entity.healthPercentage;
	}

	public void onUnitDied(GameObject gameObject)
	{
		hideInformation ();
	}
		
}
