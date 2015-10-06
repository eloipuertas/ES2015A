using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Storage;
using Utils;

public class Selectable : SubscribableActor<Selectable.Actions, Selectable>
{
	
	public enum Actions { SELECTED, DESELECTED };

    private Rect selectedRect = new Rect();
    private Texture2D selectedBox;
    bool currentlySelected { get; set; }
    private float healthRatio = 1f;
    private bool updateHealthRatio = true;
    private bool entityMoving = true;

	public Selectable() { }

    //Pendiente
    //IGameEntity gameEntity;

    public override void Start()
    {
        base.Start();

        //Pendiente
        //gameEntity = this.GetComponent<IGameEntity>();
        selectedBox = SelectionOverlay.CreateTexture();
        currentlySelected = false;
    }

    protected virtual void Update() { }

    protected virtual void LateUpdate()
    {
        bool updateSomething = false;

        // the GameEntity is moving
        if(entityMoving)
        {
            // calculates the box
            selectedRect = SelectionOverlay.CalculateBox(GetComponent<Collider>());
            updateSomething = true;
        }

        if (updateHealthRatio)
        {
            //Pendiente
            //healthRatio = gameEntity.healthPercentage() / 100f;
            updateSomething = true;
            // doesn't update until gets the callback
            updateHealthRatio = false;
        }


        if(updateSomething) SelectionOverlay.UpdateTexture(selectedBox, healthRatio);
    }

    protected virtual void OnGUI()
    {
        if (currentlySelected)
        {
            DrawSelection();
        }
    }

    public virtual void Select(Player player)
    {
        //only handle input if currently selected

        Selectable oldObject = player.SelectedObject;

        if ( !this.Equals(oldObject))
        {
            // old object selection is now false (if exists)
            if (oldObject) oldObject.currentlySelected = false;
            // player selected object is now this current selectable object
            player.SelectedObject = this;
            this.currentlySelected = true;
            //Debug pursposes
            //Pendiente
            //Debug.Log(gameEntity.info.name);
            registerEntityCallbacks();

			fire (Actions.SELECTED);

			updateActorInformation();

        }
    }

	private void updateActorInformation() 
	{
		Transform information = getHUDInformationComponent ();
		if (information != null) {
			Transform txtActorName = information.transform.FindChild ("ActorName");
			Transform txtActorRace = information.transform.FindChild ("ActorRace");
			Transform txtActorHealth = information.transform.FindChild ("ActorHealth");
			Transform txtActorImage = information.transform.FindChild ("ActorImage");
			Transform sliderActorHealth = information.transform.FindChild ("ActorHealthSlider");
					
			IGameEntity entity = gameObject.GetComponent<IGameEntity> ();
			txtActorName.gameObject.GetComponent<Text> ().text = entity.info.name;
			txtActorName.gameObject.GetComponent<Text> ().enabled = true;

			txtActorRace.gameObject.GetComponent<Text> ().text = entity.info.race.ToString ();
			txtActorRace.gameObject.GetComponent<Text> ().enabled = true;

			txtActorHealth.gameObject.GetComponent<Text> ().text = entity.healthPercentage.ToString () + "/100";
			txtActorHealth.gameObject.GetComponent<Text> ().enabled = true;

			txtActorImage.gameObject.GetComponent<Image> ().color = new Color (0, 0, 1, 1);
			txtActorImage.gameObject.GetComponent<Image> ().enabled = true;

			sliderActorHealth.gameObject.GetComponent<Slider> ().value = entity.healthPercentage;
			sliderActorHealth.gameObject.GetComponent<Slider> ().enabled = true;

			Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
			sliderBackground.GetComponent<Image>().enabled = true;
		}
	}

	private void hideActorInformation() 
	{
		Transform information = getHUDInformationComponent ();
		if (information != null) 
		{
			Transform txtActorName = information.transform.FindChild("ActorName");
			Transform txtActorRace = information.transform.FindChild ("ActorRace");
			Transform txtActorHealth = information.transform.FindChild("ActorHealth");
			Transform txtActorImage = information.transform.FindChild ("ActorImage");
			Transform sliderActorHealth = information.transform.FindChild ("ActorHealthSlider");

			txtActorName.gameObject.GetComponent<Text>().enabled = false;
			txtActorRace.gameObject.GetComponent<Text>().enabled = false;
			txtActorHealth.gameObject.GetComponent<Text>().enabled = false;
			txtActorImage.gameObject.GetComponent<Image>().enabled = false;
			sliderActorHealth.gameObject.GetComponent<Slider>().enabled = false;
			sliderActorHealth.gameObject.GetComponent<Slider>().value = 0;

			Transform sliderBackground = sliderActorHealth.transform.FindChild ("Background");
			sliderBackground.GetComponent<Image>().enabled = false;
		}
	}

	private Transform getHUDInformationComponent() 
	{
		GameObject hud = GameObject.Find ("HUD");	
		if (hud != null) {
			Transform information = hud.transform.FindChild ("Information");
			if (information != null) {
				return information;
			}
		}

		return null;
	}

    private void registerEntityCallbacks()
    {
        //TODO
    }
    private void unregisterEntityCallbacks()
    {
        //TODO
    }
    public virtual void Deselect()
    {
        currentlySelected = false;
		hideActorInformation ();
    }

    private void DrawSelection()
    {
        GUI.DrawTexture(selectedRect, selectedBox);
    }
}
