using UnityEngine;
using System.Collections;

public class TutorialLogic : MonoBehaviour
{


    // Use this for initialization
    void Start()
    {

    }

    // Check if we have clicked esc to go to main menu.
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            Application.LoadLevel(0); // Scene = is Main Menu
    }
}
