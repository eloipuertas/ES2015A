using UnityEngine;
using System.Collections;
using Storage;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;

public class GameInformation : MonoBehaviour
{

    private Races playerRace;
    private GameObject currentHud = null;

    public enum GameMode { CAMPAIGN, SKIRMISH };
    private GameMode gameMode;

    private Battle game;
    public int Difficulty { get; set; }

    private static string pauseMenuPrefab;

    private static int BUTTON_ROWS = 3;
    private static int BUTTON_COLUMNS = 4;

    private static float Arial_Fifteen_Size_X = 7.8f;
    private static int Arial_Fifteen_Size_y = 18;
    private static int HOVER_TEXT_SIZE = 15;
    private static Color HOVER_TEXT_COLOR = Color.white;
    private static Color HOVER_TEXT_BACKGROUND = Color.black;
    private static float BACKGROUND_ALPHA = 0.5f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadHUD()
    {
        // TODO: reload different hud while playing
        //if (currentHud) Destroy(currentHud);
        //Debug.LogError("Creando el hud");
        switch (playerRace)
        {
            case Races.ELVES:
                LoadElfHUD();
                break;
            case Races.MEN:
                LoadHumanHUD();
                break;
        }

        currentHud.AddComponent<HUDPopulationInfo>(); // Adds the text to show population stats.  
        LoadActionButtons();
    }

    private void LoadElfHUD()
    {

        currentHud = Instantiate((GameObject)Resources.Load("HUD-Elf"));
        currentHud.name = "HUD";
        Instantiate((GameObject)Resources.Load("HUD_EventSystem")).name = "HUD_EventSystem";
    }

    private void LoadHumanHUD()
    {
        currentHud = Instantiate((GameObject)Resources.Load("HUD-Human"));
        currentHud.name = "HUD";
        Instantiate((GameObject)Resources.Load("HUD_EventSystem")).name = "HUD_EventSystem";
    }

    public void LoadActionButtons()
    {
        GameObject actionPanel = GameObject.Find("HUD/actions");

        if (!actionPanel) return;
        //IGameEntity entity = gameObject.GetComponent<IGameEntity>();
        var rectTransform = actionPanel.GetComponent<RectTransform>();
        var panelTransform = rectTransform.transform;
        var size = rectTransform.sizeDelta;
        var globalScaleXY = new Vector2(rectTransform.lossyScale.x, rectTransform.lossyScale.y);
        var extents = Vector2.Scale(size, globalScaleXY) / 2.0f;
        var buttonExtents = new Vector2(extents.x / BUTTON_COLUMNS, extents.y / BUTTON_ROWS);
        var position = rectTransform.position;
        var point = new Vector2(position.x - extents.x, position.y + extents.y);
        for (int i = 0; i < BUTTON_COLUMNS * BUTTON_ROWS; i++)
        {
            var buttonCenter = point + buttonExtents * (2 * (i % BUTTON_COLUMNS) + 1);
            buttonCenter.y = point.y - (buttonExtents.y * (2 * (i / BUTTON_COLUMNS) + 1));
            //CreateButton(rectTransform, buttonCenter, buttonExtents, ability, actionMethod, !abilityObj.isActive);
            var buttonObject = new GameObject("Button " + i);
            buttonObject.tag = "ActionButton";
            buttonObject.layer = 5; // UI Layer

            var image = buttonObject.AddComponent<Image>();
            image.tag = "ActionButton";
            //image.rectTransform.SetParent(panelTransform);
            image.rectTransform.localScale = panelTransform.localScale;
            image.rectTransform.sizeDelta = 1.5f * buttonExtents;
            image.rectTransform.position = buttonCenter;
            image.enabled = false;
            //image.rectTransform.position = transform.position;
            //image.sprite = CreateSprite(ability, image.rectTransform.sizeDelta);
            //Debug.LogError("Button position: " + center);
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transform.SetParent(actionPanel.transform);
            button.interactable = false;

            var eventTrigger = buttonObject.AddComponent<EventTrigger>();
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback = new EventTrigger.TriggerEvent();
            UnityEngine.Events.UnityAction<BaseEventData> enterCall = new UnityEngine.Events.UnityAction<BaseEventData>(mouseEnter);
            enterEntry.callback.AddListener(enterCall);
            eventTrigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback = new EventTrigger.TriggerEvent();
            UnityEngine.Events.UnityAction<BaseEventData> exitCall = new UnityEngine.Events.UnityAction<BaseEventData>(mouseExit);
            exitEntry.callback.AddListener(exitCall);
            eventTrigger.triggers.Add(exitEntry);

            eventTrigger.enabled = false;
            //buttonObject.GetComponent<Renderer>().enabled= false;
        }
        actionPanel.GetComponent<Image>().enabled = false;
    }


    public void mouseEnter(BaseEventData baseEvent)
    {
        var oldTooltip = GameObject.Find("tooltip");
        if (oldTooltip)
        {
            Destroy(oldTooltip);
        }
        PointerEventData data = baseEvent as PointerEventData;
        GameObject panel = GameObject.Find("HUD/actions");

        var panelTransform = panel.GetComponent<RectTransform>();
        var panelSize = panelTransform.sizeDelta;
        var panelGlobalScaleXY = new Vector2(panelTransform.lossyScale.x, panelTransform.lossyScale.y);
        var panelExtents = Vector2.Scale(panelSize, panelGlobalScaleXY) / 2.0f;
        var buttonExtents = new Vector2(panelExtents.x / BUTTON_COLUMNS, panelExtents.y / BUTTON_ROWS);
        var panelPosition = panelTransform.position;
        var panelOrigin = new Vector2(panelPosition.x - panelExtents.x, panelPosition.y + panelExtents.y);

        var name = data.pointerEnter.name;
        var button = GameObject.Find(name);
		var buttonImage = button.GetComponent<Image>();
		var buttonTransform = buttonImage.rectTransform;
		if (buttonImage.name != "Sell" && buttonImage.name != "Recruit Explorer"
		    && buttonImage.name!="Rotate") {
			buttonImage.sprite = CreateHoverSprite (buttonImage.name, buttonTransform.sizeDelta);     
		}
        var tooltip = new GameObject("tooltip");
        var canvas = tooltip.AddComponent<Canvas>();
        var tooltipTransform = tooltip.GetComponent<RectTransform>();
        tooltip.AddComponent<GraphicRaycaster>();
        tooltipTransform.position = buttonTransform.position;
        tooltipTransform.localScale = panelTransform.localScale;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;

        var desplazamiento = new Vector2(Screen.width / 4.0f, Screen.height / 4.0f);
        var desplazamientoInterno = new Vector2(buttonTransform.position.x, buttonTransform.position.y) - panelOrigin;
        var aspectRatio = Screen.width / Screen.height;

        var imageObject = new GameObject("Background");
        var image = imageObject.AddComponent<Image>();
        var imageTransform = imageObject.GetComponent<RectTransform>();
        imageTransform.SetParent(tooltipTransform);
        imageTransform.localPosition = new Vector2((desplazamiento.x - buttonExtents.x) + (Math.Abs(desplazamientoInterno.x) - Math.Abs(buttonExtents.x)), -desplazamiento.y * aspectRatio - (Math.Abs(desplazamientoInterno.y) - Math.Abs(buttonExtents.y)));
        imageTransform.localScale = panelTransform.localScale;
        imageTransform.sizeDelta = new Vector2(Arial_Fifteen_Size_X * name.Length, Arial_Fifteen_Size_y);

        var color = HOVER_TEXT_BACKGROUND;
        color.a = BACKGROUND_ALPHA;
        image.color = color;

        var descripcion = new GameObject("Descripcion");
        var text = descripcion.AddComponent<Text>();
        var descripcionTransform = descripcion.GetComponent<RectTransform>();
        descripcionTransform.SetParent(imageTransform);
        descripcionTransform.localPosition = Vector3.zero;
        descripcionTransform.sizeDelta = new Vector2(Arial_Fifteen_Size_X * name.Length, Arial_Fifteen_Size_y);
        descripcionTransform.localScale = imageTransform.localScale;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = ArialFont;
        text.material = ArialFont.material;
        text.text = name;
        text.fontSize = HOVER_TEXT_SIZE;
        text.color = HOVER_TEXT_COLOR;
        text.enabled = true;

        //text.supportRichText = true;
    }

    private void mouseExit(BaseEventData baseEvent)
    {
		PointerEventData data = baseEvent as PointerEventData;
        var tooltip = GameObject.Find("tooltip");
		var name = data.pointerEnter.name;
        var button = GameObject.Find(name);
		var buttonImage = button.GetComponent<Image>();
		var buttonTransform = buttonImage.rectTransform;
		buttonImage.sprite = CreateSprite (buttonImage.name, buttonTransform.sizeDelta);  
        Destroy(tooltip);
    }


	Sprite CreateSprite(String ability, Vector2 size)
	{
		Sprite newImg = null;
		char separator = Path.AltDirectorySeparatorChar;
		
		Texture2D image;
		
		String file = "ActionButtons" + separator + ability.Replace(" ", "_");
		image = Resources.Load(file) as Texture2D;
		if (image)
		{
			//newImg = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(center.x, center.y));
			newImg = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.0f, 0.0f));
		}
		
		return newImg;
	}


	Sprite CreateHoverSprite(String ability, Vector2 size)
	{
		Sprite newImg = null;
		char separator = Path.AltDirectorySeparatorChar;
		
		Texture2D image;
		
		String file = "ActionButtons" + separator +"Hover"+ separator + ability.Replace(" ", "_")+"_Hover_Enabled";
		image = Resources.Load(file) as Texture2D;
		if (image)
		{
			//newImg = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(center.x, center.y));
			newImg = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.0f, 0.0f));
		}
		
		return newImg;
	}


    public void SetPlayerRace(int race)
    {
        playerRace = (Races)race;
        switch (playerRace)
        {
            case Races.ELVES:
                pauseMenuPrefab = "PauseMenu-Elf";
                break;
            case Races.MEN:
                pauseMenuPrefab = "PauseMenu-Human";
                break;
        }
    }

    public void SetPlayerRace(Races race)
    {
        playerRace = race;
        switch (playerRace)
        {
            case Races.ELVES:
                pauseMenuPrefab = "PauseMenu-Elf";
                break;
            case Races.MEN:
                pauseMenuPrefab = "PauseMenu-Human";
                break;
        }
    }

    public Races GetPlayerRace()
    {
        return playerRace;
    }

    public string GetPauseMenuPrefabPath()
    {
        return pauseMenuPrefab;
    }

    public void setGameMode(GameMode mode)
    {
        gameMode = mode;
        hardcodedBattle();    // HACK Sets the campaign while the real functionality isn't implemented
    }

    public GameMode getGameMode()
    {
        return gameMode;
    }

    public void SetBattle(Battle battle)
    {
        game = battle;
    }

    public Battle GetBattle()
    {
        return game;
    }

    private void hardcodedBattle()
    {
        game = new Battle();
        Battle.MissionDefinition.TargetType t = new Battle.MissionDefinition.TargetType();
        t.unit = UnitTypes.HERO;
        game.AddMission(Battle.MissionType.DESTROY, 1, EntityType.UNIT, t, 0, true, "");
        Battle.PlayerInformation player = new Battle.PlayerInformation(Races.MEN);
        player.AddBuilding(BuildingTypes.STRONGHOLD, 777, 779, EntityStatus.IDLE);
        player.AddUnit(UnitTypes.HERO, 825.6648f, 806.5628f);
        player.SetInitialResources(2000, 2000, 2000, 2000);
        game.AddPlayerInformation(player);
        player = new Battle.PlayerInformation(Races.ELVES);
        player.AddUnit(UnitTypes.HERO, 331.35f, 575.81f);
        player.AddBuilding(BuildingTypes.STRONGHOLD, 283.7f, 562.5f, EntityStatus.IDLE);
        player.SetInitialResources(2000, 2000, 2000, 2000);
        game.AddPlayerInformation(player);
        game.SetWorldResources(5000, 5000, 5000);
    }

    private Battle.PlayerInformation initializePlayer(Storage.Races race, string strongholdGameObject)
    {
        GameObject stronghold;
        Battle.PlayerInformation player = new Battle.PlayerInformation(race);
        stronghold = GameObject.Find(strongholdGameObject);
        player.AddBuilding(BuildingTypes.STRONGHOLD, stronghold.transform.position.x, stronghold.transform.position.z, EntityStatus.IDLE);
        player.AddUnit(UnitTypes.HERO, stronghold.transform.position.x - 50, stronghold.transform.position.z);
        player.SetInitialResources(2000, 2000, 2000, 2000);
        return player;
    }

    public void SetStoryBattle()
    {
        game = new Battle();
        Races enemyRace = playerRace == Races.ELVES ? Races.MEN : Races.ELVES;
        game.AddPlayerInformation(initializePlayer(playerRace, "Cube_Player_Stronghold"));
        game.AddPlayerInformation(initializePlayer(enemyRace, "Cube_Enemy_Stronghold"));
        game.SetWorldResources(5000, 5000, 5000);
        Battle.MissionDefinition.TargetType t = new Battle.MissionDefinition.TargetType();
        t.building = BuildingTypes.STRONGHOLD;
        game.AddMission(Battle.MissionType.DESTROY, 1, EntityType.BUILDING, t, 0, true, "");
    }
}
