using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class EntityActionsController : MonoBehaviour
{

    private static int Button_Rows = 3;
    private static int Button_Columns = 3;
    private Boolean button = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    void OnMouseDown()
    {
        CheckActionButtons();
        CreateUI();
    }


    /// <summary>
    /// Method to create a new UI Element.In our case only buttons.
    /// </summary>
    void CreateUI()
    {
        GameObject actionPanel = GameObject.Find("actions");
        IGameEntity entity = gameObject.GetComponent<IGameEntity>();
        if (Input.GetMouseButtonDown(0))
        {
            if (!button)
            {

                //actionPanel.GetComponent = null;
                var rectTransform = actionPanel.GetComponent<RectTransform>();
                var extents = 0.9f * rectTransform.sizeDelta / 2.0f;
                var buttonExtents = new Vector2(extents.x / Button_Columns, extents.y / Button_Rows);
                var position = rectTransform.position;
                var point = new Vector2(position.x - extents.x, position.y + extents.y);
                var actions = entity.info.actions;
                var nactions = actions.Count;

                for (int i = 0; i < nactions; i++)
                {
                    String action = actions[i].name;
                    IAction actionObj = entity.getAction(action);
                    if (actionObj.isUsable && !actionObj.isActive)
                    {
                        actionObj.enable();
                        UnityAction actionMethod = new UnityAction(() => SayHello());
                        var buttonCenter = point + buttonExtents * (2 * (i % Button_Columns) + 1);
                        buttonCenter.y = point.y - (buttonExtents.y * (2 * (i / Button_Rows) + 1));
                        CreateButton(actionPanel, buttonCenter, buttonExtents, action, actionMethod);
                    }

                }
                button = true;
            }
            else
            {
                var actions = entity.info.actions;
                var nactions = actions.Count;
                Debug.Log("Numero de acciones :" + nactions);
                for (int i = 0; i < nactions; i++)
                {
                    String action = actions[i].name;
                    IAction actionObj = entity.getAction(action);
                    actionObj.disable();
                    Debug.Log("Desactivando el objeto");
                    var objeto = GameObject.Find(actions[i].name);
                    if (objeto != null)
                    {
                        Destroy(objeto);
                    }
                }
                button = false;
            }

        }
    }

    void CheckActionButtons()
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
    void CreateButton(GameObject panel, Vector2 center, Vector2 extends, String action, UnityAction actionMethod)
    {
        var canvasObject = new GameObject(action);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.tag = "ActionButton";
        canvasObject.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var buttonObject = new GameObject("Button");
        var image = buttonObject.AddComponent<Image>();
        image.transform.parent = canvas.transform;
        image.rectTransform.sizeDelta = extends * 1.5f;
        image.rectTransform.position = center;
        image.color = new Color(1f, .3f, .3f, .5f);
        Debug.Log("Image position: " + image.transform.position);


        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => actionMethod());
        var textObject = new GameObject("ActionText");
        textObject.transform.parent = buttonObject.transform;
        var text = textObject.AddComponent<Text>();
        text.rectTransform.sizeDelta = extends * 1.5f;
        text.rectTransform.position = center;
        text.text = action;
        text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
        text.fontSize = 10;
        text.color = Color.yellow;
        text.alignment = TextAnchor.MiddleCenter;
    }

    /// <summary>
    /// Exemple method of action
    /// </summary>
    void SayHello()
    {
        Debug.Log("Hello everybody!");
    }
}
