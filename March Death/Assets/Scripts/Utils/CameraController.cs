using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

    private GameObject followingGameObject;
    private bool isManualControlEnabled;
    private float speed;

	void Start () {
        this.isManualControlEnabled = true;
        this.speed = 5f;
	}
	
	void Update () {
	}

    /// <summary>
    ///  Make the camera look at certain position.
    /// </summary>
    /// <param name="target"></param>
    public void lookAtPoint(Vector3 target)
    {
    }

    /// <summary>
    /// Make the camera look at certain game object.
    /// </summary>
    /// <param name="target"></param>
    public void lookGameObject(GameObject target)
    {     
    }
    
    /// <summary>
    /// Make the camera follow a game object.
    /// </summary>
    /// <param name="target"></param>
    public void followGameObject(GameObject target)
    {
        this.followingGameObject = target;
    }
    
    /// <summary>
    /// Stops the camera follow
    /// </summary>
    public void stopFollowing()
    {
        this.followingGameObject = null;
    }

    /// <summary>
    /// Enables the manual control with standard WASD and arrow keys.
    /// </summary>
    public void enableManualControl()
    {
        this.followingGameObject = null;
        this.isManualControlEnabled = true;
    }


    /// <summary>
    /// Disables the manual control of the camera
    /// </summary>
    public void disableManualControl()
    {
        this.isManualControlEnabled = false;
    }

    /// <summary>
    /// Set the speed of the camera for the manual control
    /// </summary>
    /// <param name="newSpeed"></param>
    public void setCameraSpeed(float newSpeed)
    {
        this.speed = newSpeed;
    }

    
    /// <summary>
    /// Make the camera smoootly travel and look at certain game object at certain speed.
    /// Usefull to make some kind of camera animations 
    /// </summary>
    /// <param name="target">The desired target that you want to reach</param>
    /// <param name="speed">Optional if not provided the speed will be default camera speed</param>
    public void smothTravelToTarget(GameObject target, float speed = -1)
    {
    }


    /// <summary>
    /// Make the camera smoootly travel and look at certain position at certain speed.
    /// Usefull to make some kind of camera animations 
    /// </summary>
    /// <param name="target">The desired position that you want to look at.</param>
    /// <param name="speed">Optional if not provided the speed will be default camera speed</param>
    public void smoothTravelToPosition(Vector3 position, float speed = -1)
    {
    }

}
