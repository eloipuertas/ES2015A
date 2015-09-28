using UnityEngine;

/// <summary>
/// Attach this script to the main camera and it will be able to be controlled like an Isometric Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

    private const float CAMERA_MAX_ZOOM = 10;
    private const float CAMERA_MIN_ZOOM = 100;

    private Vector3 cameraOffset;
    private GameObject followingGameObject;
    private GameObject cameraContainer;
    private bool isManualControlEnabled;

    //Camera smooth translation atributes
    private Vector3 lerpStart, lerpEnd;
    private float lerpPosition, lerpTime;
    private bool isLerping;

    private float _cameraSpeed;
    private float _mouseWeelZoomSensitivity;
    private float _defaultLerpTime;

    public float defaultLerpTime
    {
        get { return _defaultLerpTime; }
    }

    public float mouseWeelZoomSensitivity
    {
        get { return _mouseWeelZoomSensitivity; }
    }

    public float cameraSpeed
    {
        get { return _cameraSpeed; }
    }

    void Start()
    {
        setupCamera();
        _cameraSpeed = 5f;
        _mouseWeelZoomSensitivity = 5f;
        _defaultLerpTime = 2f;
        lerpTime = 20f;
        isManualControlEnabled = true;
        isLerping = false;
    }

    void Update()
    {
        if (followingGameObject != null)
        {
            lookGameObject(followingGameObject);
        }

        if (isManualControlEnabled)
        {
            handlePlayerInput();
        }

        if (isLerping)
        {
            handleSmoothTravel();
        }
    }

    /// <summary>
    ///  Make the camera look at certain position.
    /// </summary>
    /// <param name="target"></param>
    public void lookAtPoint(Vector3 target)
    {
        stopAllAutomaticTasks();
        Vector3 newCameraPos = target + cameraOffset;
        cameraContainer.transform.position = newCameraPos;
    }

    /// <summary>
    /// Make the camera look to certain game object.
    /// </summary>
    /// <param name="target"></param>
    public void lookGameObject(GameObject target)
    {
        stopAllAutomaticTasks();
        Vector3 newCameraPos = target.transform.position + cameraOffset;
        cameraContainer.transform.position = newCameraPos;
    }

    /// <summary>
    /// Make the camera follow a game object.
    /// </summary>
    /// <param name="target"></param>
    public void followGameObject(GameObject target)
    {
        stopAllAutomaticTasks();
        followingGameObject = target;
    }
    
    /// <summary>
    /// Stops the camera follow
    /// </summary>
    public void stopFollowing()
    {
        followingGameObject = null;
    }

    /// <summary>
    /// Enables the manual control with standard WASD and arrow keys.
    /// </summary>
    public void enableManualControl()
    {
        stopAllAutomaticTasks();
        isManualControlEnabled = true;
    }


    /// <summary>
    /// Disables the manual control of the camera
    /// </summary>
    public void disableManualControl()
    {
        isManualControlEnabled = false;
    }

    /// <summary>
    /// Set the speed of the camera for the manual control
    /// </summary>
    /// <param name="newSpeed"></param>
    public void setCameraSpeed(float newSpeed)
    {
        if(newSpeed > 0)
        {
            _cameraSpeed = newSpeed;
        }
    }


    /// <summary>
    /// Sets the sensitivity of the mouse to make zoom
    /// </summary>
    /// <param name="newWeelSensitivity">New sensitivity</param>
    public void setZoomSpeed(float newWeelSensitivity)
    {
        if(newWeelSensitivity > 0)
        {
            _mouseWeelZoomSensitivity = newWeelSensitivity;
        }
    }


    /// <summary>
    /// Make the camera smoootly travel and look at certain game object at certain time controled in seconds.
    /// Usefull to make some kind of camera animations 
    /// </summary>
    /// <param name="target">The desired target that you want to reach</param>
    /// <param name="time">Optional if not provided the time will be default camera time</param>
    public void smoothTravelToTarget(GameObject target, float time = -1)
    {
        stopAllAutomaticTasks();
        lerpStart = transform.position;
        lerpEnd = target.transform.position + cameraOffset;
        if(time == -1)
        {
            lerpTime = defaultLerpTime;
        }
        else
        {
            lerpTime = time;
        }
        isLerping = true;
    }

    /// <summary>
    /// Make the camera smoootly travel and look at certain position at certain speed.
    /// Usefull to make some kind of camera animations 
    /// </summary>
    /// <param name="target">The desired position that you want to look at.</param>
    /// <param name="speed">Optional if not provided the speed will be default camera speed</param>
    public void smoothTravelToPosition(Vector3 position, float time = -1)
    {
        stopAllAutomaticTasks();
        lerpStart = transform.position;
        lerpEnd = position + cameraOffset;

        if (time == -1)
        {
            lerpTime = defaultLerpTime;
        }
        else
        {
            lerpTime = time;
        }

        isLerping = true;
    }



    /// <summary>
    /// Make the camera travel between to points of the map in a certain time
    /// </summary>
    /// <param name="origin"> The origin of the travel</param>
    /// <param name="end"> The end of the travel</param>
    /// <param name="time">Duration of the travel</param>
    public void smoothTravelBetweenTwoPoints(Vector3 origin, Vector3 end, float time = -1)
    {
        stopAllAutomaticTasks();
        lerpStart = origin + cameraOffset;
        lerpEnd = end + cameraOffset;

        if (time == -1)
        {
            lerpTime = defaultLerpTime;
        }
        else
        {
            lerpTime = time;
        }

        isLerping = true;
    }

    /// <summary>
    /// Sets the new smooth travel duration
    /// </summary>
    /// <param name="newSmoothTravelDuration"> new smooth travel duration in seconds </param>
    public void setDefaultSmoothTravelTime(float newSmoothTravelDuration)
    {
        if(newSmoothTravelDuration > 0)
        {
            _defaultLerpTime = newSmoothTravelDuration;
        }
    }


    /// <summary>
    /// Stops the smooth travel 
    /// </summary>
    public void stopSmoothTravel()
    {
        isLerping = false;
    }


    /// <summary>
    /// Setup the camera and look the pont 0, 0, 0
    /// </summary>
    private void setupCamera()
    {
        cameraOffset = new Vector3(-20f, 26.8f, -20f);
        Vector3 desiredCameraPosition = new Vector3(transform.position.x, cameraOffset.y, transform.position.z);
        cameraContainer = new GameObject("Camera");
        transform.localEulerAngles = new Vector3(45f, 0f, 0f);
        gameObject.transform.parent = cameraContainer.transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0, transform.localEulerAngles.z);
        cameraContainer.transform.position = desiredCameraPosition;
        Camera.main.fieldOfView = 30;
        cameraContainer.transform.Rotate(Vector3.up, 45f);
        lookAtPoint(Vector3.zero);
    }


    /// <summary>
    /// Sets the new zoom of the camera
    /// </summary>
    /// <param name="newZoom"> A number between 10 (max zoom) and 100 (min zoom) </param>
    public void setCameraZoom(float newZoom)
    {
        float fov = Mathf.Clamp(newZoom, CAMERA_MAX_ZOOM, CAMERA_MIN_ZOOM);
        if(newZoom > 0)
        {
            Camera.main.fieldOfView = fov;
        }
    }


    /// <summary>
    /// Internal Camera method used to perform an smooth travel in certain time between to points
    /// </summary>
    private void handleSmoothTravel()
    {
        lerpPosition += Time.deltaTime / lerpTime;
        transform.position = Vector3.Lerp(lerpStart, lerpEnd, lerpPosition);

        if (transform.position.Equals(lerpEnd))
        {
            isLerping = false;
        }
    }


    /// <summary>
    /// Internal Camera method used to handle player input.
    /// </summary>
    private void handlePlayerInput()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            cameraContainer.transform.Translate(Vector3.forward * Time.deltaTime * _cameraSpeed);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            cameraContainer.transform.Translate(Vector3.left * Time.deltaTime * _cameraSpeed);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            cameraContainer.transform.Translate(Vector3.back * Time.deltaTime * _cameraSpeed);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            cameraContainer.transform.Translate(Vector3.right * Time.deltaTime * _cameraSpeed);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            stopSmoothTravel();
        }

        handleZoom();
    }


    /// <summary>
    /// Internal Camera method used to handle player Zoom
    /// </summary>
    private void handleZoom()
    {
        float fov = Camera.main.fieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * _mouseWeelZoomSensitivity;
        setCameraZoom(fov);
    }


    /// <summary>
    /// Stops all the actual automatic functions
    /// </summary>
    private void stopAllAutomaticTasks()
    {
        stopFollowing();
        stopSmoothTravel();
    }
}
