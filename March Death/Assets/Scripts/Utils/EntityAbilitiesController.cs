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
		//Register to selectable actions
		Subscriber<Selectable.Actions, Selectable>.get.registerForAll (Selectable.Actions.SELECTED, onActorSelected);
		Subscriber<Selectable.Actions, Selectable>.get.registerForAll (Selectable.Actions.DESELECTED, onActorDeselected);
    }

    // Update is called once per frame
    void Update()
    {

    }

	public void onActorSelected(System.Object obj)
	{
		destroyButtons ();
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
		var extents = 0.9f * rectTransform.sizeDelta / 2.0f;
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
				UnityAction actionMethod = new UnityAction(() => SayHello());

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
	
	Sprite CreateSprite(String ability)
    {
        char separator = Path.DirectorySeparatorChar;
        Sprite newImg=null;
        Texture2D tex = null;
        byte[] fileData;

        String sPath = Application.dataPath +separator+"Resources"+separator+ "ActionButtons" + separator;
        string sName = sPath+ability+".png";

        if (File.Exists(sName))
        {
            fileData = File.ReadAllBytes(sName);
            tex = new Texture2D(100, 100, TextureFormat.RGB24, false);
            tex.LoadImage(fileData);

            newImg = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        return newImg;
    }
}
