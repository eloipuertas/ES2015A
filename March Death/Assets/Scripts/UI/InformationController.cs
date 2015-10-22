using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Storage;
using Utils;

public class InformationController : MonoBehaviour {

	private Player player;

	//objects for one unit information
	private Text txtActorName;
	private Text txtActorRace;
	private Text txtActorHealth;
	private Image imgActorDetail;
	private Slider sliderActorHealth;
    
    //objects for multiple units information
    int columns = 10;
	int rows = 2;
	Vector2 buttonSize;
	Vector2 initialPoint;
	
	// Use this for initialization
	void Start () 
	{
        GameObject gameInformationObject = GameObject.Find("GameInformationObject");
        player = GameObject.FindGameObjectWithTag("GameController").GetComponent("Player") as Player;


        //Register to selectable actions
        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.SELECTED, onUnitSelected, new ActorSelector()
        {
            registerCondition = (checkRace) => {
                Debug.Log(checkRace);
                Debug.Log(checkRace.GetComponent<IGameEntity>());
                Debug.Log(checkRace.GetComponent<IGameEntity>().info);
                Debug.Log(checkRace.GetComponent<IGameEntity>().info.race);
                return checkRace.GetComponent<IGameEntity>().info.race == gameInformationObject.GetComponent<GameInformation>().GetPlayerRace();
                }
        });

        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.DESELECTED, onUnitDeselected, new ActorSelector()
        {
            registerCondition = (checkRace) => checkRace.GetComponent<IGameEntity>().info.race == gameInformationObject.GetComponent<GameInformation>().GetPlayerRace()
        });


        //Init menu components used for show info for one unit
        Transform information = GameObject.Find ("HUD").transform.FindChild ("Information");
		txtActorName = information.transform.FindChild("ActorName").gameObject.GetComponent<Text>();
		txtActorRace = information.transform.FindChild ("ActorRace").gameObject.GetComponent<Text>();
		txtActorHealth = information.transform.FindChild("ActorHealth").gameObject.GetComponent<Text>();
		imgActorDetail = information.transform.FindChild ("ActorImage").gameObject.GetComponent<Image>();
		sliderActorHealth = information.transform.FindChild ("ActorHealthSlider").gameObject.GetComponent<Slider>();

		//Precalculate objects used for show info for multiple units
		RectTransform rectTransform = GameObject.Find("Information").GetComponent<RectTransform>();
		Vector2 panelSize = rectTransform.sizeDelta;
		Vector2 center = rectTransform.position;
		buttonSize = new Vector2(panelSize.x / columns, panelSize.y / rows);
		initialPoint = new Vector2(center.x - panelSize.x / 2, center.y + panelSize.y / 2);

		//Default is hidden
		HideInformation ();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	private void ShowInformation(GameObject gameObject) 
	{
		IGameEntity entity = gameObject.GetComponent<IGameEntity> ();

		txtActorName.text = entity.info.name;
		txtActorName.enabled = true;
		txtActorRace.text = entity.info.race.ToString ();
		txtActorRace.enabled = true;
		txtActorHealth.text = entity.healthPercentage.ToString () + "/100";
		txtActorHealth.enabled = true;	
		/* Ready for next sprint
		Texture2D actorTexture = (Texture2D)Resources.Load ("SelectionTexture");
		Sprite image = Sprite.Create(actorTexture, new Rect(0, 0, actorTexture.width, actorTexture.height), new Vector2(0.5f, 0.5f));
		//imgActorDetail.color = new Color (0, 0, 1, 1);
		imgActorDetail.enabled = true;
		imgActorDetail.sprite = image;
		*/
		sliderActorHealth.value = entity.healthPercentage;
		sliderActorHealth.enabled = true;
		Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
		sliderBackground.GetComponent<Image>().enabled = true;
	}

	private void ShowMultipleInformation() {

		/*
		 * Ready for next Sprint
		HideInformation ();

		for (int i = 0; i < player.SelectedObjects.Count && i < columns * rows; i++)
		{
			Selectable selectable = (Selectable)player.SelectedObjects [i];
			CreateButton(i, selectable);
		}
		*/
	}

	private void HideInformation() 
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

	private void CreateButton(int i, Selectable selectable) {

		int line = 1;
		if (i >= 10) line = 2;

		Vector2 buttonCenter = new Vector2();
		buttonCenter.x = initialPoint.x + buttonSize.x / 2 + (buttonSize.x * (i % columns));
		buttonCenter.y = initialPoint.y + (buttonSize.y / 2) - buttonSize.y * line;

		IGameEntity entity = selectable.GetComponent<IGameEntity>();
		CreateButton(buttonCenter, buttonSize, entity.info.race.ToString());
	}
	
	void CreateButton(Vector2 center, Vector2 size, String text)
	{
		var canvasObject = new GameObject(text);
		var canvas = canvasObject.AddComponent<Canvas>();
		canvas.tag = "ActionButton";
		canvasObject.AddComponent<GraphicRaycaster>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		
		var buttonObject = new GameObject("Button");
		var image = buttonObject.AddComponent<Image>();
		image.transform.parent = canvas.transform;
		image.rectTransform.sizeDelta = size * 0.9f;
		image.rectTransform.position = center;
		image.color = new Color(1f, .3f, .3f, .5f);
		
		var button = buttonObject.AddComponent<Button>();
		button.targetGraphic = image;
		
		var textObject = new GameObject("ActionText");
		textObject.transform.parent = buttonObject.transform;
		var lblText = textObject.AddComponent<Text>();
		lblText.rectTransform.sizeDelta = size * 0.9f;
		lblText.rectTransform.position = center;
		lblText.text = text;
		lblText.font = Resources.FindObjectsOfTypeAll<Font>()[0];
		lblText.fontSize = 10;
		lblText.color = Color.white;
		lblText.alignment = TextAnchor.MiddleCenter;
	}


	public void onUnitSelected(System.Object obj)
	{
        GameObject gameObject = (GameObject) obj;

		//Check if is simple click or multiple
		if (player.SelectedObjects.Count > 1)
		{
			HideInformation();
			ShowMultipleInformation();

		} else
		{
			ShowInformation(gameObject);
		}
		//TODO: parse actor type (building / unit)


		//Register for unit events
		IGameEntity entity = gameObject.GetComponent<IGameEntity>();

        entity.doIfUnit(unit =>
        {
            unit.register(Unit.Actions.DAMAGED, onUnitDamaged);
            unit.register(Unit.Actions.DIED, onUnitDied);
        });
	}

    public void onUnitDeselected(System.Object obj)
    {
        GameObject gameObject = (GameObject)obj;

        //Check if is simple click or multiple
        if (player.SelectedObjects.Count > 1)
        {
            ShowMultipleInformation();

        } else if (player.SelectedObjects.Count == 1)
        {
            ShowInformation(gameObject);
        } else
        {
            HideInformation();
        }

        //Unregister unit events
        IGameEntity entity = gameObject.GetComponent<IGameEntity>();

        entity.doIfUnit(unit =>
        {
            unit.unregister(Unit.Actions.DAMAGED, onUnitDamaged);
            unit.unregister(Unit.Actions.DIED, onUnitDied);
        });
	}

	public void onUnitDamaged(System.Object obj)
	{
        GameObject gameObject = (GameObject) obj;
		IGameEntity entity = gameObject.GetComponent<IGameEntity> ();
		sliderActorHealth.value = entity.healthPercentage;
	}

    public void onUnitDied(System.Object obj)
	{
		HideInformation ();
	}
		
}
