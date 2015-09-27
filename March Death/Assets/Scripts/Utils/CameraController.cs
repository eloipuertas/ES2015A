using UnityEngine;

/// <summary>
/// Attach this script to the main camera and it will be able to be controlled like an Isometric Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

    private Vector3 cameraOffset;
    private GameObject followingGameObject;
    private GameObject cameraContainer;
    private bool isManualControlEnabled;
    private float speed;
    private float mouseWeelZoomSensitivity;

    void Start()
    {
        setupCamera();
        this.isManualControlEnabled = true;
        this.speed = 5f;
        this.mouseWeelZoomSensitivity = 5f;
    }

    void Update()
    {
        if (this.followingGameObject != null)
        {
            this.lookGameObject(followingGameObject);
        }

        if (this.isManualControlEnabled)
        {
            handlePlayerInput();
        }
    }

    /// <summary>
    ///  Make the camera look at certain position.
    /// </summary>
    /// <param name="target"></param>
    public void lookAtPoint(Vector3 target)
    {
        this.followingGameObject = null;
        Vector3 newCameraPos = target + cameraOffset;
        this.cameraContainer.transform.position = newCameraPos;
    }

    /// <summary>
    /// Make the camera look to certain game object.
    /// </summary>
    /// <param name="target"></param>
    public void lookGameObject(GameObject target)
    {
        Vector3 newCameraPos = target.transform.position + cameraOffset;
        this.cameraContainer.transform.position = newCameraPos;
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
    public void smoothTravelToTarget(GameObject target, float speed = -1)
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

    private void handlePlayerInput()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            this.cameraContainer.transform.Translate(Vector3.forward * Time.deltaTime * this.speed);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            this.cameraContainer.transform.Translate(Vector3.left * Time.deltaTime * this.speed);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            this.cameraContainer.transform.Translate(Vector3.back * Time.deltaTime * this.speed);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            this.cameraContainer.transform.Translate(Vector3.right * Time.deltaTime * this.speed);
        }

        handleZoom();

    }

    /// <summary>
    /// Setup the camera and look the pont 0, 0, 0
    /// </summary>
    private void setupCamera()
    {
        this.cameraOffset = new Vector3(-20f, 26.8f, -20f);
        Vector3 desiredCameraPosition = new Vector3(this.transform.position.x, this.cameraOffset.y, this.transform.position.z);
        this.cameraContainer = new GameObject("Camera");
        this.transform.localEulerAngles = new Vector3(45f, 0f, 0f);
        this.gameObject.transform.parent = this.cameraContainer.transform;
        this.transform.localPosition = Vector3.zero;
        this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x, 0, this.transform.localEulerAngles.z);
        this.cameraContainer.transform.position = desiredCameraPosition;
        Camera.main.fieldOfView = 30;
        this.cameraContainer.transform.Rotate(Vector3.up, 45f);
        this.lookAtPoint(Vector3.zero);
    }

    private void handleZoom()
    {
        float fov = Camera.main.fieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * this.mouseWeelZoomSensitivity;
        fov = Mathf.Clamp(fov, 10, 100);
        Camera.main.fieldOfView = fov;
    }

}
