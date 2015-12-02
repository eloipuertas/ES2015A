using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Storage;
using Utils;

public partial class InformationController : MonoBehaviour {

	private float REPAINT_TIME = 0.3f;
	private int TITLE_SIZE = 8;
	private int VALUE_SIZE = 15;
	private float timer = 0.3f;

	private enum Panel { CIVIL, UNIT, STRONGHOLD, BARRACK, RESOURCE, SQUAD };
	private GameObject windowInfo;
	private Font ArialFont;
	private ArrayList example;
	List<GameObject> currentTitles;
	List<GameObject> currentValues;
	private Panel currentPanel;
	private MonoBehaviour currentObject;
	
	void OnGUI()
	{
		if (Event.current.type.Equals(EventType.repaint))
		{
			if (windowInfo.GetComponent<Image>().enabled)
			{
				if (timer > REPAINT_TIME)
				{
					switch(currentPanel) {
					case Panel.CIVIL:
						break;
					case Panel.UNIT:
						displayInformationForUnit((IGameEntity)currentObject);
						break;
					case Panel.STRONGHOLD:
						displayInformationForStronghold((IGameEntity)currentObject);
						break;
					case Panel.BARRACK:
						displayInformationForBarrack((IGameEntity)currentObject);
						break;
					case Panel.RESOURCE:
						displayInformationForResource((Resource)currentObject);
						break;
					case Panel.SQUAD:
						//displayInformationForSquad((Squad)currentObject);
						break;
					}
					timer = 0f;
				}
				timer += Time.deltaTime;
			}
		}
	}
	
	private void hideExtendedInformationPanel() {

		//Hide window info
		windowInfo.GetComponent<Image>().enabled = false;

		//Remove old titles and values
		foreach (GameObject title in currentTitles) {
			Destroy (title);
		}
		foreach (GameObject value in currentValues) {
			Destroy (value);
		}
	}

	private void displayInformationForUnit(IGameEntity entity) {

		//Remove previous values
		hideExtendedInformationPanel();
		    
		//Display window info
		windowInfo.GetComponent<Image>().enabled = true;

		currentPanel = Panel.UNIT;
		currentObject = (MonoBehaviour)entity;
		
		List<String> titles = new List<String>();
		titles.Add("Attack rate");
		titles.Add("Attack range");
		titles.Add("Movement rate");
		titles.Add("Resistance");
		titles.Add("Sight range");
		titles.Add("Strenght");
		titles.Add("Weapon Ability");
		List<String> values = new List<String>();
		values.Add(entity.info.unitAttributes.attackRate.ToString());
		values.Add(entity.info.unitAttributes.attackRange.ToString());
		values.Add(entity.info.unitAttributes.movementRate.ToString());
		values.Add(entity.info.unitAttributes.resistance.ToString());
		values.Add(entity.info.unitAttributes.sightRange.ToString());
		values.Add(entity.info.unitAttributes.strength.ToString());
		values.Add(entity.info.unitAttributes.weaponAbility.ToString());
		displayInformation(titles, values);
	}

	private void displayInformationForStronghold(IGameEntity entity) 
	{

		//Remove previous values
		hideExtendedInformationPanel();

		//Display window info
		windowInfo.GetComponent<Image>().enabled = true;

		currentPanel = Panel.STRONGHOLD;
		currentObject = (MonoBehaviour)entity;

		List<String> titles = PopulationInfo.get.GetBuildingKeys().GetRange(0,10);
		List<String> values = PopulationInfo.get.GetBuildingValues().GetRange(0,10);
		displayInformation(titles, values);
	}

	private void displayInformationForResource(Resource resource) {

		//Remove previous values
		hideExtendedInformationPanel();
		
		//Display window info
		windowInfo.GetComponent<Image>().enabled = true;

		currentPanel = Panel.RESOURCE;
		currentObject = (MonoBehaviour)resource;
		
		List<String> titles = new List<String>();
		titles.Add("Max workers");
		titles.Add("Workers");
		titles.Add("Max production");
		titles.Add("Current production");
		titles.Add("Capacity");
		List<String> values = new List<String>();
		values.Add(resource.info.resourceAttributes.maxUnits.ToString());
		values.Add(resource.HUD_currentWorkers.ToString());
		values.Add(resource.HUD_productionRate.ToString());
		values.Add(resource.HUD_currentProductionRate.ToString());
		values.Add(resource.HUD_storeSize.ToString());

		displayInformation(titles, values);
	}

	private void displayInformationForBarrack(IGameEntity entity) {
		displayInformationForStronghold(entity);
		
	}

	private void displayInformationForSquad(Squad squad) {
		//Remove previous values
		hideExtendedInformationPanel();
		
		//Display window info
		windowInfo.GetComponent<Image>().enabled = true;

		currentPanel = Panel.SQUAD;
		//currentObject = squad;
		
		List<String> titles = new List<String>();
		titles.Add("Units");
		List<String> values = new List<String>();
		values.Add(squad.Units.Count.ToString());
		displayInformation(titles, values);
		
	}

	private void displayInformation(List<String> titles, List<String> values) 
	{
		int maxY = (int)windowInfo.GetComponent<RectTransform>().sizeDelta.y;
		int step = maxY / titles.Count - 4; // raul_hack
		
		for (int i = 0; i < titles.Count; i++)
		{
			string title = titles[i];
			GameObject labelTitle = new GameObject(title);
			labelTitle.AddComponent<Text>();
			labelTitle.transform.Translate(0, maxY/2 - i*step - 25, 0); // raul_hack
			labelTitle.GetComponent<RectTransform>().SetParent(windowInfo.GetComponent<RectTransform>(), false);
			Text titleText = labelTitle.GetComponent<Text>();
			titleText = customizeText(titleText, title, TITLE_SIZE);
			currentTitles.Add(labelTitle);
			
			string value = values[i];
			GameObject labelValue = new GameObject(title);
			labelValue.AddComponent<Text>();
			labelValue.transform.Translate(0, maxY / 2 - i * step - 45, 0); // raul_hack
			labelValue.GetComponent<RectTransform>().SetParent(windowInfo.GetComponent<RectTransform>(), false);
			Text valueText = labelValue.GetComponent<Text>();
			valueText = customizeText(valueText, value.ToString(), VALUE_SIZE);
			currentValues.Add(labelValue); 
		}
	}

	private Text customizeText(Text label, string text, int fontSize)
	{
		label.text = text;
		label.font = ArialFont;
		label.fontStyle = FontStyle.Normal;
		label.fontSize = fontSize;
		label.color = Color.white;
		label.alignment = TextAnchor.MiddleCenter;
		
		return label;
	}
}
