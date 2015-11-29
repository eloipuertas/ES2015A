using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Storage;
using Utils;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;

public partial class InformationController : MonoBehaviour {
	
	//objects for creation units queue
	int creationQueueButtonsCount;
	Vector2 creationQueueButtonSize;
	Vector2 creationQueueInitialPoint;
	Vector2 scaledUnitCreationPanel;
	Resource currentResource;

	ArrayList creationQueueButtons = new ArrayList();

	// Update is called once per frame
	void Update () 
	{
		//Check if array have buttons -> clear array on destroy
		if (creationQueueButtons.Count > 0) {
			GameObject buttonCanvas = (GameObject) creationQueueButtons[0];
			GameObject button = buttonCanvas.transform.Find ("MultiSelectionButtonButton").gameObject;
			Image image = button.GetComponent<Image> ();

			if (currentResource != null) {
				float percentage = currentResource.getcreationUnitPercentage();
				image.fillAmount = 1 - percentage / 100f;
			}
		}
	}

	private void ShowCreationQueue() 
	{
		DestroyUnitCreationButtons();

		if (currentResource != null) {
			creationQueueButtonsCount = currentResource.getNumberElements();
		
			for (int i = 0; i < creationQueueButtonsCount; i++) {
				Vector2 buttonCenter = new Vector2();
				buttonCenter.x = creationQueueInitialPoint.x + scaledUnitCreationPanel.x / 2f + (creationQueueButtonSize.x * i) + creationQueueButtonSize.x / 2f;
				buttonCenter.y = creationQueueInitialPoint.y - scaledUnitCreationPanel.y;
				GameObject button = CreateCreationUnitButton(buttonCenter);
				creationQueueButtons.Add(button);
			}
		}
	}

	private GameObject CreateCreationUnitButton(Vector2 center) 
	{
		GameObject canvasObject = new GameObject("MultiSelectionButtonCanvas");
		Canvas canvas = canvasObject.AddComponent<Canvas>();
		canvas.tag = "MultiSelectionButton";
		canvasObject.AddComponent<GraphicRaycaster>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		GameObject buttonObject = new GameObject("CreationUnitImage");
		Image image = buttonObject.AddComponent<Image>();
		image.transform.parent = canvas.transform;
		image.rectTransform.sizeDelta = creationQueueButtonSize * 0.9f;
		image.rectTransform.position = center;
		Texture2D texture = (Texture2D)Resources.Load ("InformationImages/MEN_civil");
		if (texture) {
			image.sprite = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2 (0.5f, 0.5f));
		}

		GameObject shadowObject = new GameObject("MultiSelectionButtonButton");
		Image shadow = shadowObject.AddComponent<Image>();
		shadow.transform.parent = canvas.transform;
		shadow.rectTransform.sizeDelta = creationQueueButtonSize * 0.9f;
		shadow.rectTransform.position = center;
		shadow.type = Image.Type.Filled;
		shadow.fillMethod = Image.FillMethod.Radial360;
		shadow.transform.Rotate(180,0,0);
		Texture2D shadowTexture = (Texture2D)Resources.Load ("creationUnitShadow");
		if (shadowTexture) {
			shadow.sprite = Sprite.Create (shadowTexture, new Rect (0, 0, shadowTexture.width, shadowTexture.height), new Vector2 (0.5f, 0.5f));
		}
		return canvasObject;
	}

	private void DestroyUnitCreationButtons() {
		creationQueueButtons.Clear();
		GameObject[] buttons = GameObject.FindGameObjectsWithTag("MultiSelectionButton");
		if (buttons != null)
		{
			foreach (GameObject button in buttons)
			{
				Destroy(button);
			}
		}
	}	
}
