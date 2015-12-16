using UnityEngine;
using System.Collections;

public class TutorialLogic : MonoBehaviour
{

    



    

    public static TutorialLogic instance = null;
    // Use this for initialization
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("loading tutorial");
        }


    }

    public void Update()
    {


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {

            Application.LoadLevel(1);

        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            Application.LoadLevel(4);

        }
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
