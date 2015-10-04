using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class ItemMenuController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        CreateUI();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void CreateUI()
    {
        GameObject panel = GameObject.FindGameObjectWithTag("panel");
        IGameEntity entity = gameObject.GetComponent<IGameEntity>();

        var actions = entity.info.actions;
        var nactions = actions.length;

        var renderer = this.gameObject.GetComponent<Renderer>();
        var minPoint = renderer.bounds.min;
        var maxPoint = renderer.bounds.max;
        var point = new Vector3(minPoint.x, maxPoint.y, 0);
        var size = renderer.bounds.extents / nactions;
        
        for(int i = 0; i < nactions; i++)
        {
            UnityAction actionMethod = new UnityAction(Method);
            var action = actions[i];
            var centerPoint = point + size * (2 * i + 1);
            centerPoint.y = minPoint.y;
            centerPoint.z = minPoint.z;
            CreateButton(panel, centerPoint, size, action, actionMethod);

        }
    }

    void CreateButton(GameObject panel, Vector3 center, Vector3 extends, String action, UnityAction actionMethod)
    {
        var canvasObject = new GameObject("Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvasObject.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.WorldSpace;

        var buttonObject = new GameObject("Button");
        var image = buttonObject.AddComponent<Image>();
        image.transform.parent = canvas.transform;
        image.rectTransform.sizeDelta = extends * 2;
        image.rectTransform.position = center;
        image.color = new Color(1f, .3f, .3f, .5f);
        Debug.Log("Image position: " + image.transform.position);


        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => actionMethod());
        var textObject = new GameObject("Text");
        textObject.transform.parent = buttonObject.transform;
        var text = textObject.AddComponent<Text>();
        text.rectTransform.sizeDelta = extends * 2;
        text.rectTransform.anchoredPosition = center;
        text.text = action;
        text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
        text.fontSize = 10;
        text.color = Color.yellow;
        text.alignment = TextAnchor.MiddleCenter;
        Debug.Log("Button position: " + button.transform.position);
    }
}
