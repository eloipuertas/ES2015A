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

        // Displays the Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenuLogic.TogglePauseMenu();
        }

        foreach (KeyValuePair<Ability,bool> tuple in EntityAbilitiesController.affordable_buttons)
        {
            if (Input.GetKeyUp(tuple.Key.keyBinding) && tuple.Value)
            {
                tuple.Key.enable();
            }
        }

    }
}
