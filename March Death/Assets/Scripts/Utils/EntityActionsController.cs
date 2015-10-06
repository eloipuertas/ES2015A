using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class EntityActionsController : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        CreateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var renderer = this.gameObject.GetComponent<Renderer>();
            var localMousePosition = Input.mousePosition;
            localMousePosition.z = renderer.bounds.center.z - Camera.main.gameObject.transform.position.z;
            var position = Camera.main.ScreenToWorldPoint(localMousePosition);

            var minPoint = renderer.bounds.min;
            var maxPoint = renderer.bounds.max;
            if (position.x < minPoint.x || position.y < minPoint.y || position.x > maxPoint.x || position.y > maxPoint.y)
            {
                Debug.Log("Has pulsado el raton en area prohibido");
                GameObject[] objetos = GameObject.FindGameObjectsWithTag("Button");
                foreach (GameObject objeto in objetos)
                {
                    Destroy(objeto);
                }
            }
        }


    }


    /// <summary>
    /// Method to create a new UI Element.In our case only buttons.
    /// </summary>
    void CreateUI()
    {
        GameObject panel = GameObject.FindGameObjectWithTag("Player");
        IGameEntity entity = gameObject.GetComponent<IGameEntity>();

        //Debug.Log("Estas accediendo a un objeto vacio: "+ panel.GetComponent<IGameEntity>().info.ToString());
        //Debug.Log("NActions : "+panel.GetComponent<IGameEntity>().info.actions.Count);
        var actions = entity.info.actions;
        var nactions = actions.Count;
        var renderer = panel.GetComponent<Renderer>();
        var minPoint = renderer.bounds.min;
        var maxPoint = renderer.bounds.max;
        var point = new Vector3(minPoint.x, maxPoint.y, 0);
        var size = renderer.bounds.extents / nactions;

        for (int i = 0; i < nactions; i++)
        {
            UnityAction actionMethod = new UnityAction(() => SayHello());
            String action = actions[i].name;
            var centerPoint = point + size * (2 * i + 1);
            centerPoint.y = minPoint.y - (renderer.bounds.extents.y / 2.0f);
            centerPoint.z = minPoint.z;
            CreateButton(panel, centerPoint, size, action, actionMethod);

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
    void CreateButton(GameObject panel, Vector3 center, Vector3 extends, String action, UnityAction actionMethod)
    {
        var canvasObject = new GameObject("Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvasObject.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.WorldSpace;

        var buttonObject = new GameObject("Button");
        var image = buttonObject.AddComponent<Image>();
        image.transform.parent = canvas.transform;
        image.rectTransform.sizeDelta = extends * 1.8f;
        image.rectTransform.position = center;
        image.color = new Color(1f, .3f, .3f, .5f);
        Debug.Log("Image position: " + image.transform.position);


        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => actionMethod());
        var textObject = new GameObject("Text");
        textObject.transform.parent = buttonObject.transform;
        var text = textObject.AddComponent<Text>();
        text.rectTransform.sizeDelta = extends * 1.8f;
        text.rectTransform.anchoredPosition = center;
        text.text = action;
        text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
        text.fontSize = 10;
        text.color = Color.yellow;
        text.alignment = TextAnchor.MiddleCenter;
        Debug.Log("Button position: " + button.transform.position);
    }

    /// <summary>
    /// Exemple method of action
    /// </summary>
    void SayHello()
    {
        Debug.Log("Hello everybody!");
    }
}
