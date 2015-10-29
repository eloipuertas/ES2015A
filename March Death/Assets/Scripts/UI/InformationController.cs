﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Storage;
using Utils;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;

public class InformationController : MonoBehaviour {

	private Player player;

	private const string IMAGES_PATH = "InformationImages";

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
	
	Dictionary<Selectable, GameObject> multiselectionButtons = new Dictionary<Selectable, GameObject>();
	
	// Use this for initialization
	void Start () 
	{
        GameObject gameInformationObject = GameObject.Find("GameInformationObject");
        player = GameObject.FindGameObjectWithTag("GameController").GetComponent("Player") as Player;


        //Register to selectable actions
        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.SELECTED, onUnitSelected, new ActorSelector()
        {
            registerCondition = (checkRace) => checkRace.GetComponent<IGameEntity>().info.race == gameInformationObject.GetComponent<GameInformation>().GetPlayerRace()
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
		RectTransform rectTransform = GameObject.Find("Information").transform.FindChild ("background").GetComponent<RectTransform>();
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

		char separator = Path.DirectorySeparatorChar;
		string path = IMAGES_PATH + separator + entity.getRace () + "_" + entity.info.name;
		Texture2D texture = (Texture2D)Resources.Load (path);
		if (texture) {
			imgActorDetail.enabled = true;
			imgActorDetail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		}

		sliderActorHealth.value = entity.healthPercentage;
		sliderActorHealth.enabled = true;
		Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
		sliderBackground.GetComponent<Image>().enabled = true;
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

	private void ShowMultipleInformation() 
	{
		ArrayList selectedObjects = player.getSelectedObjects();
		for (int i = 0; i < selectedObjects.Count && i < columns * rows; i++)
		{
			double lineDivision = (double)(i / columns);
			int line = (int)Math.Ceiling(lineDivision) + 1;
			
			Vector2 buttonCenter = new Vector2();
			buttonCenter.x = initialPoint.x + buttonSize.x / 2 + (buttonSize.x * (i % columns));
			buttonCenter.y = initialPoint.y + (buttonSize.y / 2) - buttonSize.y * line;

			Selectable selectable = (Selectable)selectedObjects[i];

			//Reuse buttons to avoid create and destroy
			if (multiselectionButtons.ContainsKey(selectable)) {
				GameObject button = multiselectionButtons[selectable];
				modifyButton(button, buttonCenter);
			} else {
				GameObject button = CreateButton(buttonCenter, selectable);
				multiselectionButtons.Add(selectable, button);
			}
		}
	}

	private void modifyButton(GameObject buttonCanvas, Vector2 center) {
		GameObject button = buttonCanvas.transform.Find ("Button").gameObject;
		Image image = button.GetComponent<Image> ();
		image.rectTransform.position = center;
		Text text = button.transform.FindChild ("MultiSelectionText").GetComponent<Text> ();
		text.rectTransform.position = center;
	}

	private GameObject CreateButton(Vector2 buttonCenter, Selectable selectable) {
		IGameEntity entity = selectable.GetComponent<IGameEntity>();

		UnityAction actionMethod = new UnityAction(() =>
		{
			selectable.SelectUnique();
		});

		return CreateButton(buttonCenter, entity.info.race.ToString(), actionMethod);
	}
	
	private GameObject CreateButton(Vector2 center, String text, UnityAction actionMethod) 
	{
		GameObject canvasObject = new GameObject(text);
		Canvas canvas = canvasObject.AddComponent<Canvas>();
		canvas.tag = "MultiSelectionButton";
		canvasObject.AddComponent<GraphicRaycaster>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		
		GameObject buttonObject = new GameObject("Button");
		var image = buttonObject.AddComponent<Image>();
		image.transform.parent = canvas.transform;
		image.rectTransform.sizeDelta = buttonSize * 0.9f;
		image.rectTransform.position = center;
		image.color = new Color(1f, .3f, .3f, .5f);
		
		Button button = buttonObject.AddComponent<Button>();
		button.onClick.AddListener(() => actionMethod());
		button.targetGraphic = image;
		
		GameObject textObject = new GameObject("MultiSelectionText");
		textObject.transform.parent = buttonObject.transform;
		Text lblText = textObject.AddComponent<Text>();
		lblText.rectTransform.sizeDelta = buttonSize * 0.9f;
		lblText.rectTransform.position = center;
		lblText.text = text;
		lblText.font = Resources.FindObjectsOfTypeAll<Font>()[0];
		lblText.fontSize = 10;
		lblText.color = Color.white;
		lblText.alignment = TextAnchor.MiddleCenter;
		return canvasObject;
	}

	void DestroyButtons()
	{
		multiselectionButtons.Clear ();
		GameObject[] buttons = GameObject.FindGameObjectsWithTag("MultiSelectionButton");
		if (buttons != null)
		{
			foreach (GameObject button in buttons)
			{
				Destroy(button);
			}
		}
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
			DestroyButtons();
			ShowInformation(gameObject);
		}

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
			DestroyButtons();
            ShowInformation(gameObject);
        } else
        {
			DestroyButtons();
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

	public void Clear()
	{
		Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.SELECTED, onUnitSelected);
		Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.DESELECTED, onUnitDeselected);
	}
		
}
