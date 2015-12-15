using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Storage;
using Utils;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections.Specialized;

public partial class InformationController : MonoBehaviour {
	
	//objects for creation units queue
	Vector2 creationQueueButtonSize;
	Vector2 creationQueueInitialPoint;
	Vector2 scaledUnitCreationPanel;
	Resource currentResource;
	Barrack currentBarrack;

	ArrayList creationQueueButtons = new ArrayList();

	// Update is called once per frame
	void Update () 
	{
		//Check if array have buttons -> clear array on destroy
		if (creationQueueButtons.Count > 0) {
			GameObject buttonCanvas = (GameObject) creationQueueButtons[0];
			if (buttonCanvas != null) {
				GameObject button = buttonCanvas.transform.Find ("UnitCreationShadow").gameObject;
				Image image = button.GetComponent<Image> ();
				
				if (currentResource != null) {
					float percentage = currentResource.getcreationUnitPercentage();
					image.fillAmount = 1 - percentage / 100f;
				} else if (currentBarrack != null) {
					float percentage = currentBarrack.getcreationUnitPercentage();
					image.fillAmount = 1 - percentage / 100f;
				}
			}
		}
	}

	private void ShowCreationQueue() 
	{
		DestroyUnitCreationButtons();
		
		if (currentResource != null || currentBarrack != null ) {

			UnitTypes[] creationUnitQueue = null;

			if (currentResource != null) {
				creationUnitQueue = new UnitTypes[currentResource.getNumberElements()];
				creationUnitQueue = currentResource.getCreationQueue().ToArray();
			} else if (currentBarrack != null) {
				creationUnitQueue = new UnitTypes[currentBarrack.getNumberElements()];
				creationUnitQueue = currentBarrack.getCreationQueue().ToArray();
			}

			for (int i = 0; i < creationUnitQueue.Length; i++) {
				UnitTypes type = creationUnitQueue[i];
				Vector2 buttonCenter = new Vector2();
				buttonCenter.x = creationQueueInitialPoint.x + scaledUnitCreationPanel.x / 2f + (creationQueueButtonSize.x * i) + creationQueueButtonSize.x / 2f;
				buttonCenter.y = creationQueueInitialPoint.y - scaledUnitCreationPanel.y;
				GameObject button = CreateCreationUnitButton(buttonCenter, type);
				creationQueueButtons.Add(button);
			}
		}
	}

	private GameObject CreateCreationUnitButton(Vector2 center, UnitTypes type) 
	{
		GameObject canvasObject = new GameObject("UnitCreationButtonCanvas");
		Canvas canvas = canvasObject.AddComponent<Canvas>();
		canvas.tag = "UnitCreationButton";
		canvasObject.AddComponent<GraphicRaycaster>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		GameObject buttonObject = new GameObject("CreationUnitImage");
		Image image = buttonObject.AddComponent<Image>();
		image.transform.parent = canvas.transform;
		image.rectTransform.sizeDelta = creationQueueButtonSize * 0.9f;
		image.rectTransform.position = center;
		Sprite buttonImage = GetImageForType(type);
		if (buttonImage != null)
		{
			image.sprite = buttonImage;
		}
		else
		{
			image.color = new Color(1f, 1f, 1f, 1f);
		}

		Button button = buttonObject.AddComponent<Button>();
		button.targetGraphic = image;
		button.onClick.AddListener(() =>
		{
			if (currentResource != null) {
				currentResource.cancelUnitQueue();
			} else if (currentBarrack != null) {
				currentBarrack.cancelUnitQueue();
			}

			ShowCreationQueue();
		});


		GameObject shadowObject = new GameObject("UnitCreationShadow");
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

	private Sprite GetImageForType(UnitTypes type)
	{

		char separator = '/';
		String entityName = "";
		switch(type) {
		case UnitTypes.CAVALRY:
			entityName = "cavalry";
			break;
		case UnitTypes.CIVIL:
			entityName = "civil";
			break;
		case UnitTypes.HEAVY:
			entityName = "heavy soldier";
			break;
		case UnitTypes.HERO:
			entityName = "Hero";
			break;
		case UnitTypes.LIGHT:
			entityName = "light soldier";
			break;
		case UnitTypes.MACHINE:
			entityName = "machine";
			break;
		case UnitTypes.SPECIAL:
			entityName = "special";
			break;
		case UnitTypes.THROWN:
			entityName = "thrown";
			break;
		default:
			entityName = "";
			break;
		}

		string path = IMAGES_PATH + separator + playerRace + "_" + entityName;
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

	private void DestroyUnitCreationButtons() {
		creationQueueButtons.Clear();
		GameObject[] buttons = GameObject.FindGameObjectsWithTag("UnitCreationButton");
		if (buttons != null)
		{
			foreach (GameObject button in buttons)
			{
				Destroy(button);
			}
		}
	}	
}
