using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Storage;
using Utils;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;

public partial class InformationController : MonoBehaviour
{
	private const string IMAGES_PATH = "InformationImages";

	Races playerRace;

	//objects for one unit information
	private Text txtActorName;
	private Text txtActorRace;
	private Text txtActorHealth;
	private Image imgActorDetail;
	private Slider sliderActorHealth;

	//objects for multiple units information
	int multiselectionColumns = 10;
	int multiselectionRows = 2;
	Vector2 multiselectionButtonSize;
	Vector2 multiselectionInitialPoint;

	Dictionary<Selectable, GameObject> multiselectionButtons = new Dictionary<Selectable, GameObject>();

	// Use this for initialization
	void Start ()
	{
        //Register to selectable actions
        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.SELECTED, onUnitSelected, new ActorSelector()
        {
            registerCondition = (checkRace) => BasePlayer.isOfPlayer(checkRace.GetComponent<IGameEntity>())
        });

        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.DESELECTED, onUnitDeselected, new ActorSelector()
        {
            registerCondition = (checkRace) => BasePlayer.isOfPlayer(checkRace.GetComponent<IGameEntity>())
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
		Vector2 globalScaleXY = new Vector2(rectTransform.lossyScale.x, rectTransform.lossyScale.y);
		panelSize = Vector2.Scale(panelSize, globalScaleXY);
		multiselectionButtonSize = new Vector2(panelSize.x / multiselectionColumns, panelSize.y / multiselectionRows);
		multiselectionInitialPoint = new Vector2(center.x - panelSize.x / 2, center.y + panelSize.y / 2);

		//Create button to generate squad controls
		rectTransform = GameObject.Find("Information").transform.FindChild ("SquadButtons").GetComponent<RectTransform>();
		panelSize = rectTransform.sizeDelta;
		center = rectTransform.position;
		globalScaleXY = new Vector2(rectTransform.lossyScale.x, rectTransform.lossyScale.y);
		panelSize = Vector2.Scale(panelSize, globalScaleXY);
        squadsButtonSize = new Vector2(panelSize.x / squadsColumns, panelSize.y / squadsRows);
        squadsInitialPoint = new Vector2(center.x - panelSize.x / 2, center.y + panelSize.y / 2);
        MAX_SQUADS_BUTTONS = squadsColumns * squadsRows;

        //Inicializate parameters for unit creation buttons
		rectTransform = GameObject.Find("Information").transform.FindChild("UnitCreationPanel").GetComponent<RectTransform>();
		panelSize = rectTransform.sizeDelta;
		center = rectTransform.position;
		globalScaleXY = new Vector2(rectTransform.lossyScale.x, rectTransform.lossyScale.y);
		unitCreationPanel = Vector2.Scale(panelSize, globalScaleXY);
		creationQueueButtonSize = new Vector2(unitCreationPanel.x / 5, unitCreationPanel.y);
		creationQueueInitialPoint = new Vector2(center.x - unitCreationPanel.x, center.y + unitCreationPanel.y);

        //Inicializate window extended info
        windowInfo = GameObject.Find("HUD/windowInfo");
        ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        currentTitles = new List<GameObject>();
        currentValues = new List<GameObject>();

        //Default is hidden
        HideInformation();
        hideExtendedInformationPanel();
	}

	private void ShowInformation(GameObject gameObject)
	{
		IGameEntity entity = gameObject.GetComponent<IGameEntity> ();

		txtActorName.text = entity.info.name;
		txtActorName.enabled = true;
		txtActorRace.text = entity.info.race.ToString ();
		txtActorRace.enabled = true;
		txtActorHealth.text = Math.Ceiling(entity.healthPercentage).ToString () + "/100";
		txtActorHealth.enabled = true;

		sliderActorHealth.value = entity.healthPercentage;
		sliderActorHealth.enabled = true;
		Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
		sliderBackground.GetComponent<Image>().enabled = true;

        Sprite image = GetImageForEntity(entity);
        if (image)
        {
            imgActorDetail.enabled = true;
            imgActorDetail.sprite = image;
        }

        entity.doIfResource(resource =>
        {
            currentResource = resource;
            ShowCreationQueue();
        });
		entity.doIfBarrack(barrack =>
		                   {
			currentBarrack = barrack;
			ShowCreationQueue();
		});
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
        List<Selectable> selectedObjects = BasePlayer.player.selection.SelectedSquad.Selectables;

		for (int i = 0; i < selectedObjects.Count && i < multiselectionColumns * multiselectionRows; i++)
		{
            double lineDivision = (double)(i / multiselectionColumns);
            int line = (int)Math.Ceiling(lineDivision) + 1;

            Vector2 buttonCenter = new Vector2();
            buttonCenter.x = multiselectionInitialPoint.x + multiselectionButtonSize.x / 2 + (multiselectionButtonSize.x * (i % multiselectionColumns));
            buttonCenter.y = multiselectionInitialPoint.y + (multiselectionButtonSize.y / 2) - multiselectionButtonSize.y * line;

            Selectable selectable = selectedObjects[i];

            //Reuse buttons to avoid create and destroy
            if (multiselectionButtons.ContainsKey(selectable))
            {
                GameObject button = multiselectionButtons[selectable];
                modifyButton(button, buttonCenter);
            }
            else
            {
                GameObject button = CreateMultiselectionButton(buttonCenter, selectable);
                multiselectionButtons.Add(selectable, button);
            }
        }
		ReloadSquadGenerationButton ();
	}

	private void modifyButton(GameObject buttonCanvas, Vector2 center) {
		GameObject button = buttonCanvas.transform.Find ("MultiSelectionButtonButton").gameObject;
		Image image = button.GetComponent<Image> ();
		image.rectTransform.position = center;
		Text text = button.transform.FindChild ("MultiSelectionButtonText").GetComponent<Text> ();
		text.rectTransform.position = center;
	}

	private GameObject CreateMultiselectionButton(Vector2 buttonCenter, Selectable selectable)
    {
		IGameEntity entity = selectable.GetComponent<IGameEntity>();

		UnityAction selectUnique = new UnityAction(() =>
		{
			selectable.SelectOnlyMe();
		});

		return CreateCustomButton(buttonCenter, multiselectionButtonSize, "MultiSelectionButton", "", buttonImage: GetImageForEntity (entity), actionMethod: selectUnique);
	}

	private void DestroyButtons()
	{

		//Destroy button to generate squads
		DestroyGenerateSquadButton ();

		//destroy multiselection buttons
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
        if (!BasePlayer.player.selection.IsUnique)
        {
            DestroyButtons();
            HideInformation();
            ShowMultipleInformation();
        }
        else
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
            unit.register(Unit.Actions.HEALTH_UPDATED, onHealthUpdated);
            displayInformationForUnit(entity);

        });

		entity.doIfResource(resource =>
		{
			resource.register(Resource.Actions.DAMAGED, onUnitDamaged);
			resource.register(Resource.Actions.DESTROYED, onUnitDied);
            resource.register(Resource.Actions.CREATE_UNIT, onBuildingUnitCreated);
            resource.register(Resource.Actions.ADDED_QUEUE, onBuildingLoadNewUnit);
            resource.register(Resource.Actions.HEALTH_UPDATED, onHealthUpdated);
            displayInformationForResource(resource);
            //resource.register(Resource.Actions.LOAD_UNIT, onBuildingUnitCreated);
        });

		entity.doIfBarrack(building =>
		{
			building.register(Barrack.Actions.DAMAGED, onUnitDamaged);
			building.register(Barrack.Actions.DESTROYED, onUnitDied);
            building.register(Barrack.Actions.CREATE_UNIT, onBuildingUnitCreated);
            building.register(Barrack.Actions.ADDED_QUEUE, onBuildingLoadNewUnit);
            building.register(Barrack.Actions.HEALTH_UPDATED, onHealthUpdated);
            //TODO: reload actions on building created -> building.register(Barrack.Actions.BUILDING_FINISHED, reloadActionsPanel);
            displayInformationForStronghold(entity);
        });
    }

    public void onBuildingUnitCreated(System.Object obj)
    {
        //Destroy first button and move others to the left
        ShowCreationQueue();
    }

    public void onBuildingLoadNewUnit(System.Object obj)
    {
        //Recreate buttons
        ShowCreationQueue();
    }

    public void onUnitDeselected(System.Object obj)
	{
		GameObject gameObject = (GameObject)obj;
        hideExtendedInformationPanel();


        // Hide all info
        DestroyButtons();
        HideInformation();

		//Unregister unit events
		IGameEntity entity = gameObject.GetComponent<IGameEntity>();

		entity.doIfUnit(unit =>
		{
			unit.unregister(Unit.Actions.DAMAGED, onUnitDamaged);
			unit.unregister(Unit.Actions.DIED, onUnitDied);
            unit.unregister(Unit.Actions.HEALTH_UPDATED, onHealthUpdated);
        });

        entity.doIfResource(resource =>
        {
            resource.unregister(Resource.Actions.DAMAGED, onUnitDamaged);
            resource.unregister(Resource.Actions.DESTROYED, onUnitDied);
            resource.unregister(Resource.Actions.CREATE_UNIT, onBuildingUnitCreated);
            resource.unregister(Resource.Actions.ADDED_QUEUE, onBuildingLoadNewUnit);
            resource.unregister(Resource.Actions.HEALTH_UPDATED, onHealthUpdated);
            //resource.unregister(Resource.Actions.ADDED_QUEUE, onBuildingLoadNewUnit);

            currentResource = null;
            DestroyUnitCreationButtons();
        });

        entity.doIfBarrack(building =>
        {
            building.unregister(Barrack.Actions.DAMAGED, onUnitDamaged);
            building.unregister(Barrack.Actions.DESTROYED, onUnitDied);
            building.unregister(Barrack.Actions.CREATE_UNIT, onBuildingUnitCreated);
            building.unregister(Barrack.Actions.ADDED_QUEUE, onBuildingLoadNewUnit);
            building.unregister(Barrack.Actions.HEALTH_UPDATED, onHealthUpdated);
            //building.unregister(Barrack.Actions.BUILDING_FINISHED, reloadActionsPanel);

            currentBarrack = null;
			DestroyUnitCreationButtons();
        });
    }

    public void onUnitDamaged(System.Object obj)
    {
        updateHealth(obj);
    }

    public void onHealthUpdated(System.Object obj)
    {
        updateHealth(obj);
    }

    public void updateHealth(System.Object obj)
    {
        GameObject gameObject = (GameObject)obj;
        IGameEntity entity = gameObject.GetComponent<IGameEntity>();
        sliderActorHealth.value = entity.healthPercentage;
        txtActorHealth.text = Math.Ceiling(entity.healthPercentage).ToString() + "/100";
    }

    public void onUnitDied(System.Object obj)
	{
		HideInformation ();
	}

	void OnDestroy()
	{
		Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.SELECTED, onUnitSelected);
		Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.DESELECTED, onUnitDeselected);
	}

    private Sprite GetImageForEntity(IGameEntity entity)
    {
        char separator = '/';
        string path = IMAGES_PATH + separator + entity.getRace() + "_" + entity.info.name;

        Unit.Gender gender;
        if (entity.info.name == "Civil") {
            entity.doIfUnit(unit =>
                            {
                gender = unit.gender;
                 if (gender == Unit.Gender.FEMALE) {
                    path = IMAGES_PATH + separator + entity.getRace() + "_" + entity.info.name + "_woman";
                }
            });
        }

        Texture2D texture = (Texture2D)Resources.Load(path);
        if (texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            return null;
        }
    }

    private GameObject CreateCustomButton(Vector2 center, Vector2 size, String tag, String text = "", Sprite buttonImage = null, UnityAction actionMethod = null)
    {
		String canvasName = tag + "Canvas";
		String buttonName = tag + "Button";
		String textName = tag + "Text";
		return CreateButton(center, size, tag, text, canvasName, buttonName, textName, buttonImage, actionMethod);
	}

	private GameObject CreateButton(Vector2 center, Vector2 size, String tag = "", String text = "", String canvasName = "", String buttonName = "",
	                                String textName = "", Sprite buttonImage = null, UnityAction actionMethod = null)
    {
        GameObject canvasObject = new GameObject(canvasName);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.tag = tag;
        canvasObject.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject buttonObject = new GameObject(buttonName);
        Image image = buttonObject.AddComponent<Image>();
        image.transform.SetParent(canvas.transform, false);
        image.rectTransform.sizeDelta = size * 0.9f;
        image.rectTransform.position = center;
        if (buttonImage != null)
        {
            image.sprite = buttonImage;
        }
        else
        {
            image.color = new Color(1f, .3f, .3f, .5f);
        }

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        if (actionMethod != null)
        {
            button.onClick.AddListener(() => actionMethod());
        }

        GameObject textObject = new GameObject(textName);
        textObject.transform.SetParent(buttonObject.transform, false);
        Text lblText = textObject.AddComponent<Text>();
        lblText.rectTransform.sizeDelta = size * 0.9f;
        lblText.rectTransform.position = center;
        lblText.text = text;
        lblText.font = Resources.FindObjectsOfTypeAll<Font>()[0];
        lblText.fontSize = 10;
        lblText.color = Color.white;
        lblText.alignment = TextAnchor.MiddleCenter;
        return canvasObject;
    }
}
