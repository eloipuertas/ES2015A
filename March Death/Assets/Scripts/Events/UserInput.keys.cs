using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class UserInput
{
    public void CheckKeyboard()
    {
        //creates a new troop
        if (Input.GetKeyDown(KeyCode.T))
        {
            string key = (sManager.TroopsCount + 1).ToString();
            if (!sManager.HasTroop(key))
            {
                sManager.NewTroop(key);
            }
        }

        // selectes troop 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            string key = "1";
            if (sManager.HasTroop(key))
            {
                sManager.SelectTroop(key);
                BasePlayer.player.setCurrently(Player.status.SELECTED_UNITS);
            }
        }

        if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.E))
        {
            Vector3 position;
            RaycastHit hit;
            bool hasHit;
            position = FindHit(out hasHit, Constants.Layers.TERRAIN_MASK).point;
            GameObject gameObject = Storage.Info.get.createUnit(BasePlayer.player.race, Storage.UnitTypes.SPECIALONE, position, Quaternion.Euler(0f, 0f, 0f), 0);

            BasePlayer.player.addEntity(gameObject.GetComponent<IGameEntity>());
            Debug.Log("New SpecialOne Unit!");
            /*

            Vector3 CameraCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
            if (Physics.Raycast(CameraCenter, transform.forward, out hit, 500f, Constants.Layers.TERRAIN_MASK))
            {
                position = hit.point;
                
            }    
            */
        }

        // Displays the Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenuLogic.TogglePauseMenu();
        }

        foreach (KeyValuePair<Ability,bool> tuple in EntityAbilitiesController.get.affordable_buttons)
        {
            if (Input.GetKeyUp(tuple.Key.keyBinding) && tuple.Value)
            {
                tuple.Key.enable();
            }
        }

    }
}
