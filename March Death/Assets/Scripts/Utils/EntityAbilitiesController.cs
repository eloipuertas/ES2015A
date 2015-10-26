using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using Utils;
using System.IO;

public class EntityAbilitiesController : MonoBehaviour
{

    private static int Button_Rows = 3;
    private static int Button_Columns = 3;

    // Use this for initialization
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onActorSelected(System.Object obj)
    {
        destroyButtons();
        showActions((GameObject)obj);
    }

    public void onActorDeselected(System.Object obj)
    {
        destroyButtons();
    }

    void showActions(GameObject gameObject)
    {
        GameObject actionPanel = GameObject.Find("actions");

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

        for (int i = 0; i < nabilities; i++)
        {
            String ability = abilities[i].name;
            Ability abilityObj = entity.getAbility(ability);
            if (abilityObj.isUsable)
            {
                UnityAction actionMethod = new UnityAction(() =>
                {
                    Debug.Log(abilityObj);
                    abilityObj.enable();
                });

                var buttonCenter = point + buttonExtents * (2 * (i % Button_Columns) + 1);
                buttonCenter.y = point.y - (buttonExtents.y * (2 * (i / Button_Rows) + 1));
                CreateButton(actionPanel, buttonCenter, buttonExtents, ability, actionMethod, !abilityObj.isActive);
            }
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
    void CreateButton(GameObject panel, Vector2 center, Vector2 extends, String ability, UnityAction actionMethod, Boolean enabled)
    {
        var canvasObject = new GameObject(ability);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.tag = "ActionButton";
        canvasObject.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var buttonObject = new GameObject("Button");

        var image = buttonObject.AddComponent<Image>();
        image.transform.SetParent(canvas.transform);
        image.rectTransform.sizeDelta = extends * 1.5f;
        image.rectTransform.position = center;
        image.sprite = CreateSprite(ability);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => actionMethod());
        button.enabled = enabled;
    }

    /// <summary>
    /// Exemple method of action
    /// </summary>
	void SayHello()
    {
        Debug.Log("Hello everybody!");
    }

    Sprite CreateSprite2(String ability)
    {
        char separator = Path.DirectorySeparatorChar;
        Sprite newImg = null;
        Texture2D tex = null;
        byte[] fileData;
		

        String sPath = Application.dataPath + separator + "Resources" + separator + "ActionButtons" + separator;
        string sName = sPath + ability.Replace(" ","_") + ".png";
        if (File.Exists(sName))
        {
            fileData = File.ReadAllBytes(sName);
            tex = new Texture2D(100, 100, TextureFormat.RGB24, false);
            tex.LoadImage(fileData);

            newImg = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        return newImg;
    }


    Sprite CreateSprite(String ability)
    {
        Sprite newImg = null;
        char separator = Path.AltDirectorySeparatorChar;

        Texture2D text;

        String file = "ActionButtons" + separator + ability.Replace(" ", "_");
        text = Resources.Load(file) as Texture2D;
        if (text)
        {
            newImg = Sprite.Create(text, new Rect(0, 0, text.width, text.height), new Vector2(0.5f, 0.5f));
        }

        return newImg;
    }
	public void Clear()
	{
		Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.SELECTED, onActorSelected);
		Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.DESELECTED, onActorDeselected);
	}

}
