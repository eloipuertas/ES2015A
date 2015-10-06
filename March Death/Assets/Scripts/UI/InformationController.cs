using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Storage;
using Utils;

public class InformationController : MonoBehaviour {

	private Text txtActorName;
	private Text txtActorRace;
	private Text txtActorHealth;
	private Image imgActorDetail;
	private Slider sliderActorHealth;
	
	// Use this for initialization
	void Start () 
	{

		//Register to selectable actions
		Subscriber<Selectable.Actions, Selectable>.get.registerForAll (Selectable.Actions.SELECTED, onUnitSelected);
		Subscriber<Selectable.Actions, Selectable>.get.registerForAll (Selectable.Actions.DESELECTED, onUnitDeselected);

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

	private void showInformation(GameObject gameObject) 
	{
		IGameEntity entity = gameObject.GetComponent<IGameEntity> ();

		txtActorName.text = entity.info.name;
		txtActorName.enabled = true;
		txtActorRace.text = entity.info.race.ToString ();
		txtActorRace.enabled = true;
		txtActorHealth.text = entity.healthPercentage.ToString () + "/100";
		txtActorHealth.enabled = true;	
		imgActorDetail.color = new Color (0, 0, 1, 1);
		imgActorDetail.enabled = true;
		sliderActorHealth.value = entity.healthPercentage;
		sliderActorHealth.enabled = true;
		Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
		sliderBackground.GetComponent<Image>().enabled = true;
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

	public void onUnitSelected(GameObject gameObject)
	{
		//TODO: parse actor type (building / unit)
		showInformation(gameObject);

		//Register for unit events
		IGameEntity entity = gameObject.GetComponent<IGameEntity>();
		Unit unit = entity.toUnit ();
		unit.register(Unit.Actions.DAMAGED, onUnitDamaged);
		unit.register(Unit.Actions.DAMAGED, onUnitDied);
	}

	public void onUnitDeselected(GameObject gameObject)
	{
		hideInformation ();

		//Unregister unit events
		IGameEntity entity = gameObject.GetComponent<IGameEntity>();
		Unit unit = entity.toUnit ();
		unit.unregister(Unit.Actions.DAMAGED, onUnitDamaged);
		unit.unregister(Unit.Actions.DAMAGED, onUnitDied);
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
