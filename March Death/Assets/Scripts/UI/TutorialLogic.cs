using UnityEngine;
using System.Collections;

public class TutorialLogic : MonoBehaviour
{

    static readonly Color UP_CLICK = new Color(1.0f, 1.0f, 1.0f, 0.6f); // WHITE
    static readonly Color DOWN_CLICK = new Color(0.0f, 0.0f, 0.0f, 0.6f); // GREY
    static readonly Color ENTER_OVER = new Color(0.8f, 1.0f, 0.0f, 0.6f); // YELLOW - GREEN
    static readonly Color EXIT_OVER = new Color(1.0f, 1.0f, 1.0f, 0.6f); // WHITE
    static readonly Color YELLOW = new Color(1.0f, 0.92f, 0.016f, 1f); //YELLOW

    // Use this for initialization
    void Start()
    {

    }


    /* MOUSE OVER */

    /* This method changes the color of the object we are over on entering */
    void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = YELLOW;
        //bStillInside = true;
    }

    /* This method changes the color of the object we are over on exiting */
    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = EXIT_OVER;
        //bStillInside = false;
    }

    public void TutorialSecond()
    {
        Application.LoadLevel(4);
    }

    public void TutorialFirst()
    {
        Application.LoadLevel(1);
    }

    public void MainMenu()
    {
        Application.LoadLevel(0);
    }

}
