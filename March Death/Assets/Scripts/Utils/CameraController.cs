using UnityEngine;
using System;

/// <summary>
/// Attach this script to the main camera and it will be able to be controlled like an Isometric Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{

    private const float CAMERA_MAX_ZOOM = 5f;
    private const float CAMERA_MIN_ZOOM = 100f;
    private const float MOUSE_BOUNDS = 2f;

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
        lerpTime = 2f;
        isManualControlEnabled = true;
        isLerping = false;
        lookAtPoint(Vector3.zero);
        setCameraZoom(80f);
        setCameraSpeed(20f);
        lookAtPoint(new Vector3(1935f, 79f, 969f));
    }

    void Update()
    {
        if (isManualControlEnabled)
        {
            handlePlayerInput();
        }

        if (isLerping)
        {
            handleSmoothTravel();
        }

        if (followingGameObject != null)
        {
            lookGameObject(followingGameObject);
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
        if (newSpeed > 0)
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
        if (newWeelSensitivity > 0)
        {
            _mouseWeelZoomSensitivity = newWeelSensitivity;
        }
        else
        {
           throw new InvalidOperationException("New camera zoom speed must be a positive float!");   
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
        smoothTravelBetweenTwoPoints(transform.position, target.transform.position, time);
    }

    /// <summary>
    /// Make the camera smoootly travel and look at certain position at certain speed.
    /// Usefull to make some kind of camera animations 
    /// </summary>
    /// <param name="target">The desired position that you want to look at.</param>
    /// <param name="speed">Optional if not provided the speed will be default camera speed</param>
    public void smoothTravelToPosition(Vector3 position, float time = -1)
    {
        smoothTravelBetweenTwoPoints(transform.position, position, time);
    }


    /// <summary>
    /// Make the camera travel between to gameobjects of the map in a certain time
    /// </summary>
    /// <param name="origin"> The origin gameobject</param>
    /// <param name="end"> The destination gameobject</param>
    /// <param name="time">Duration of the travel between the game objects</param>
    public void smoothTravelBetweenTwoGameObjects(GameObject origin, GameObject end, float time = -1)
    {
        smoothTravelBetweenTwoPoints(origin.transform.position, end.transform.position);
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
        else if (time > 0)
        {
            lerpTime = time;
        }
        else
        {
            throw new InvalidOperationException("Smooth time must be a positive floating point number");
        }
        isLerping = true;
    }

    /// <summary>
    /// Sets the new smooth travel duration
    /// </summary>
    /// <param name="newSmoothTravelDuration"> new smooth travel duration in seconds </param>
    public void setDefaultSmoothTravelTime(float newSmoothTravelDuration)
    {
        if (newSmoothTravelDuration > 0)
        {
            _defaultLerpTime = newSmoothTravelDuration;
        }
        else
        {
            throw new InvalidOperationException("New smooth travel duration must be a positive float!");
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
        cameraOffset = new Vector3(-252.8f, 250.34f, -252.8f);
        Vector3 desiredCameraPosition = new Vector3(transform.position.x, cameraOffset.y, transform.position.z);
        cameraContainer = new GameObject("Camera");
        transform.localEulerAngles = new Vector3(35f, 0f, 0f);
        gameObject.transform.parent = cameraContainer.transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0, transform.localEulerAngles.z);
        cameraContainer.transform.position = desiredCameraPosition;
        cameraContainer.transform.Rotate(Vector3.up, 45f);
        Camera.main.orthographic = true;
    }


    /// <summary>
    /// Sets the new zoom of the camera
    /// </summary>
    /// <param name="newZoom"> A number between 5 (max zoom) and 100 (min zoom) </param>
    public void setCameraZoom(float newZoom)
    {
        if(newZoom > 100 || newZoom < 5)
        {
            throw new InvalidOperationException("New camera zoom must be a positive float between 5 (max zoom) and 100 (min zoom)!");
        }

        float fov = Mathf.Clamp(newZoom, CAMERA_MAX_ZOOM, CAMERA_MIN_ZOOM);
        Camera.main.orthographicSize = fov;     
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
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - MOUSE_BOUNDS )
        {
            cameraContainer.transform.Translate(Vector3.forward * Time.deltaTime * _cameraSpeed);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= MOUSE_BOUNDS )
        {
            cameraContainer.transform.Translate(Vector3.left * Time.deltaTime * _cameraSpeed);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= MOUSE_BOUNDS )
        {
            cameraContainer.transform.Translate(Vector3.back * Time.deltaTime * _cameraSpeed);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - MOUSE_BOUNDS )
        {
            cameraContainer.transform.Translate(Vector3.right * Time.deltaTime * _cameraSpeed);
        }

  
        handleZoom();
    }


    /// <summary>
    /// Internal Camera method used to handle player Zoom
    /// </summary>
    private void handleZoom()
    {
        float fov = Camera.main.orthographicSize;
        fov -= Input.GetAxis("Mouse ScrollWheel") * _mouseWeelZoomSensitivity;

        if (fov < 5f)
        {
            fov = 5f;
        }

        if (fov > 100f)
        {
            fov = 100f;
        }

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