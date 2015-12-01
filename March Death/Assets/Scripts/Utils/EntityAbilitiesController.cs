using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using Utils;
using System.IO;

public class EntityAbilitiesController : MonoBehaviour
{
    //private static int Arial_Fifteen_Size_X = 10;
    //private static int Arial_Fifteen_Size_y = 23;
    private static float Arial_Fifteen_Size_X = 7.8f;
    private static int Arial_Fifteen_Size_y = 18;
    private static int HOVER_TEXT_SIZE = 11;
    private static Color HOVER_TEXT_COLOR = Color.white;
    private static Color HOVER_TEXT_BACKGROUND = Color.black;
    private static float BACKGROUND_ALPHA = 0.5f;

    private static int Button_Rows = 3;
    private static int Button_Columns = 4;
    private static Boolean showText = false;

    public static List<Ability> abilities_on_show;

    // Use this for initialization
    void Start()
    {
#if UNITY_5_2
        Physics.queriesHitTriggers = true;
#endif
        GameObject gameInformationObject = GameObject.Find("GameInformationObject");

        //Register to selectable actions
        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.SELECTED, onActorSelected, new ActorSelector()
        {
            registerCondition = (checkRace) => checkRace.GetComponent<IGameEntity>().info.race == gameInformationObject.GetComponent<GameInformation>().GetPlayerRace()
        });
        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.DESELECTED, onActorDeselected, new ActorSelector()
        {
            registerCondition = (checkRace) => checkRace.GetComponent<IGameEntity>().info.race == gameInformationObject.GetComponent<GameInformation>().GetPlayerRace()
        });

        abilities_on_show = new List<Ability>();
    }

    public void onActorSelected(System.Object obj)
    {
		GameObject gameObject = (GameObject) obj;

        destroyButtons();
		showActions(gameObject);
	
		IGameEntity entity = gameObject.GetComponent<IGameEntity>();

		entity.doIfResource(resource => {
			resource.register(Resource.Actions.BUILDING_FINISHED, showActions);
		});
		
		entity.doIfBarrack(barrack => {
			barrack.register(Barrack.Actions.BUILDING_FINISHED, showActions);
		});
    }

    public void onActorDeselected(System.Object obj)
    {
        destroyButtons();

		GameObject gameObject = (GameObject) obj;
		IGameEntity entity = gameObject.GetComponent<IGameEntity>();

		entity.doIfResource(resource => {
			resource.unregister(Resource.Actions.BUILDING_FINISHED, showActions);
		});
		
		entity.doIfBarrack(barrack => {
			barrack.unregister(Barrack.Actions.BUILDING_FINISHED, showActions);
		});
    }

    private void showActionButtons(GameObject objeto)
    {
        IGameEntity entity = objeto.GetComponent<IGameEntity>();
        var abilities = entity.info.abilities;
        var nabilities = abilities.Count;
        for (int i = 0; i < nabilities; i++)
        {
            GameObject button = GameObject.Find("Button " + i);
            String ability = abilities[i].name;
            Ability abilityObj = entity.getAbility(ability);
            var buttonComponent = button.GetComponent<Button>();
            var image = buttonComponent.GetComponent<Image>();
            var eventTrigger = button.GetComponent<EventTrigger>();
            buttonComponent.onClick.RemoveAllListeners();

            if (abilityObj.isUsable)
            {
                // HACK: When this is fired, the button status should be updated! abilityObj.isActive might have changed...
                UnityAction actionMethod = new UnityAction(() =>
                {
                    Debug.Log("* " + abilityObj);
                    abilityObj.enable();
                });
                image.sprite = CreateSprite(ability, image.rectTransform.sizeDelta);
                buttonComponent.targetGraphic = image;
                buttonComponent.onClick.AddListener(() => actionMethod());
                image.enabled = true;
                eventTrigger.enabled = true;
                buttonComponent.interactable = true;
            }
        }
    }

	void showActions(System.Object obj)
    {
		GameObject gameObject = (GameObject) obj;
        GameObject actionPanel = GameObject.Find("HUD/actions");
        
        if (!actionPanel) return;
        IGameEntity entity = gameObject.GetComponent<IGameEntity>();
        var rectTransform = actionPanel.GetComponent<RectTransform>();
        var size = rectTransform.sizeDelta;
        var globalScaleXY = new Vector2(rectTransform.lossyScale.x, rectTransform.lossyScale.y);
        var extents = Vector2.Scale(size, globalScaleXY) / 2.0f;
        var buttonExtents = new Vector2(extents.x / Button_Columns, extents.y / Button_Rows);
        var position = rectTransform.position;
        var point = new Vector2(position.x - extents.x, position.y + extents.y);
        var abilities = entity.info.abilities;
        var nabilities = abilities.Count;

        abilities_on_show.Clear();

        for (int i = 0; i < nabilities; i++)
        {
            String ability = abilities[i].name;
            Ability abilityObj = entity.getAbility(ability);
            abilities_on_show.Add(abilityObj);

            if (abilityObj.isUsable)
            {
                // HACK: When this is fired, the button status should be updated! abilityObj.isActive might have changed...
                UnityAction actionMethod = new UnityAction(() =>
                {
                    Debug.Log(abilityObj);
                    abilityObj.enable();
                });
                var buttonCenter = point + buttonExtents * (2 * (i % Button_Columns) + 1);
                buttonCenter.y = point.y - (buttonExtents.y * (2 * (i / Button_Columns) + 1));
                CreateButton(rectTransform, buttonCenter, buttonExtents, ability, actionMethod, !abilityObj.isActive);
            }
        }
    }

    void hideActionButtons(GameObject objeto)
    {
        IGameEntity entity = objeto.GetComponent<IGameEntity>();
        var abilities = entity.info.abilities;
        var nabilities = abilities.Count;
        for (int i = 0; i < nabilities; i++)
        {
            GameObject button = GameObject.Find("Button " + i);
            var buttonComponent = button.GetComponent<Button>();
            var image = buttonComponent.GetComponent<Image>();
            var eventTrigger = button.GetComponent<EventTrigger>();
            image.enabled = false;
            eventTrigger.enabled = false;
            buttonComponent.interactable = false;

        }
    }


    void destroyButtons()
    {
        GameObject[] actionButtons = GameObject.FindGameObjectsWithTag("ActionButton");
        if (actionButtons != null)
        {
            foreach (GameObject button in actionButtons)
            {
                Destroy(button);
            }
        }
    }


    /// <summary>
    /// Methodto create a new button in a panel
    /// </summary>
    /// <param name="panel">Site where we will add a new button</param>
    /// <param name="center">The center position of the new button</param>
    /// <param name="extends">The extents of the button</param>
    /// <param name="action">Actin name</param>
    /// <param name="actionMethod">Method that will be called when we click the button</param>
    void CreateButton(RectTransform panelTransform, Vector2 center, Vector2 extends, String ability, UnityAction actionMethod, Boolean enabled)
    {
        var transform = panelTransform.transform;

        var buttonObject = new GameObject(ability);
        buttonObject.tag = "ActionButton";
        buttonObject.layer = 5; // UI Layer

        var image = buttonObject.AddComponent<Image>();
        image.tag = "ActionButton";
        //image.rectTransform.SetParent(panelTransform);
        image.rectTransform.localScale = panelTransform.localScale;
        image.rectTransform.sizeDelta = 1.5f * extends;
        image.rectTransform.position = center;
        //image.rectTransform.position = transform.position;
        image.sprite = CreateSprite(ability, image.rectTransform.sizeDelta);
        //Debug.LogError("Button position: " + center);
        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => actionMethod());

        var enterTrigger = buttonObject.AddComponent<EventTrigger>();
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback = new EventTrigger.TriggerEvent();
        UnityEngine.Events.UnityAction<BaseEventData> enterCall = new UnityEngine.Events.UnityAction<BaseEventData>(mouseEnter);
        enterEntry.callback.AddListener(enterCall);
        enterTrigger.triggers.Add(enterEntry);

        var exitTrigger = buttonObject.AddComponent<EventTrigger>();
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback = new EventTrigger.TriggerEvent();
        UnityEngine.Events.UnityAction<BaseEventData> exitCall = new UnityEngine.Events.UnityAction<BaseEventData>(mouseExit);
        exitEntry.callback.AddListener(exitCall);
        exitTrigger.triggers.Add(exitEntry);


        button.transform.SetParent(GameObject.Find("HUD/actions").transform); // We assign the parent to actions Canvas from the HUD
        button.GetComponent<RectTransform>().localPosition = new Vector3(button.GetComponent<RectTransform>().localPosition.x,
                                                                         button.GetComponent<RectTransform>().localPosition.y,
                                                                         0f);

        //button.enabled = false;
        //button.interactable = false;
        //button.enabled = false;
        //button.gameObject.SetActive(false);
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
        var buttonExtents = new Vector2(panelExtents.x / Button_Columns, panelExtents.y / Button_Rows);
        var panelPosition = panelTransform.position;
        var panelOrigin = new Vector2(panelPosition.x - panelExtents.x, panelPosition.y + panelExtents.y);

        var name = data.pointerEnter.name;
        var button = GameObject.Find(name);
        var buttonTransform = button.GetComponent<Image>().rectTransform;
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
        KeyCode code = KeyCode.Greater;
        foreach (Ability a in abilities_on_show) { if (name.Equals(a._info.name)) code = a.keyBinding; }
        text.text = name + " (" + code.ToString() + ")";
        text.fontSize = HOVER_TEXT_SIZE;
        text.color = HOVER_TEXT_COLOR;
        text.enabled = true;

        //text.supportRichText = true;
    }

    private void mouseExit(BaseEventData baseEvent)
    {
        var tooltip = GameObject.Find("tooltip");
        Destroy(tooltip);
    }

    /// <summary>
    /// Exemple method of action
    /// </summary>
	void SayHello()
    {
        Debug.Log("Hello everybody!");
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

    public void Clear()
    {
        Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.SELECTED, onActorSelected);
        Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.DESELECTED, onActorDeselected);
    }

    void OnDestroy()
    {
        Clear();
    }

}
