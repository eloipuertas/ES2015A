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
        if ( Input.GetKeyDown(KeyCode.T))
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
                player.setCurrently(Player.status.SELECTED_UNITS);
            }
        }
        // Displays the Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenuLogic.TogglePauseMenu();
        }

        foreach (Ability ab in EntityAbilitiesController.abilities_on_show)
        {
            if (Input.GetKeyUp(ab.keyBinding))
            {
                ab.enable();
            }
        }

    }
}
