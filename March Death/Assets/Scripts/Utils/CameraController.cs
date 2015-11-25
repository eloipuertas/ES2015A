using UnityEngine;
using System;


/// <summary>
/// Attach this script to the main camera and it will be able to be controlled like an Isometric Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{

    public struct MapBounds
    {
        public Vector3 minxyz;
        public Vector3 maxxyz;
    }

    public enum  CameraOrientation { NORTH_WEST, SOUTH_WEST, SOUTH_EST, NORTH_EST };
    public enum CameraInteractionState { MOVING, STOPPED }

    private const float CAMERA_MAX_ZOOM = 5f;
    private const float CAMERA_MIN_ZOOM = 80f;
    private const float MOUSE_BOUNDS = 2f;
    private const float BASE_ACCELERATION = 80f;
    private const float MAX_ACCELERATION = 200f;
    private const float SHADOW_DISTANCE = 400f;

    private Vector3 cameraOffset;
    public Vector3 getCameraOffset {
        get {
            return cameraOffset;
        }
    }
    private Vector3 lastLookedPoint;
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
    private float _camera_zoom;
    private float _acceleration;
    private Vector3 _internalDisplacement;

    private CameraOrientation _camera_orientation;
    private CameraInteractionState actual_state, last_state;

    private MapBounds map1bounds;

    

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

    public float cameraZoom
    {
        get { return _camera_zoom; }
    }

    void Start()
    {
        setupCamera();
        _cameraSpeed = 5f;
        _mouseWeelZoomSensitivity = 20f;
        _acceleration = 0f;
        _defaultLerpTime = 2f;
        lerpTime = 2f;
        isManualControlEnabled = true;
        isLerping = false;
        map1bounds.maxxyz = new Vector3(870f, 250.34f, 1067f);
        map1bounds.minxyz = new Vector3(-63f, 250.34f, 130f);
        actual_state = CameraInteractionState.STOPPED;
        last_state = CameraInteractionState.STOPPED;
        setCameraZoom(40f);
        setCameraSpeed(40f);
        lookAtPoint(new Vector3(896.4047f, 90.51f, 581.8263f));
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

    void LateUpdate()
    {
        //Ensures that camera is on the bounds of the map
        cameraContainer.transform.position = new Vector3(Mathf.Clamp(cameraContainer.transform.position.x, map1bounds.minxyz.x, map1bounds.maxxyz.x),
                    cameraContainer.transform.position.y, Mathf.Clamp(cameraContainer.transform.position.z, map1bounds.minxyz.z, map1bounds.maxxyz.z));
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
        lastLookedPoint = target;
    }

    /// <summary>
    /// Make the camera look to certain game object.
    /// </summary>
    /// <param name="target"></param>
    public void lookGameObject(GameObject target)
    {
        Vector3 newCameraPos = target.transform.position + cameraOffset;
        cameraContainer.transform.position = newCameraPos;
        lastLookedPoint = target.transform.position;
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
        cameraOffset = new Vector3(-141.554f, 140f, -141.554f);
        Vector3 desiredCameraPosition = new Vector3(transform.position.x, cameraOffset.y, transform.position.z);
        cameraContainer = new GameObject("Camera");
        transform.localEulerAngles = new Vector3(35f, 0f, 0f);
        gameObject.transform.parent = cameraContainer.transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0, transform.localEulerAngles.z);
        cameraContainer.transform.position = desiredCameraPosition;
        Camera.main.orthographic = true;
        setCameraOrientation(CameraOrientation.SOUTH_WEST);
        QualitySettings.shadowDistance = SHADOW_DISTANCE;
 
    }

    /// <summary>
    /// Used to set a new camera orientation (Usefull if the map changes its rotation) 
    /// Orientations can be NORT_WEST (used in the first demo) , SOUTH_WEST, SOUTH_EST, NORTH_EST
    /// </summary>
    /// <param name="newOrientation"></param>
    public void setCameraOrientation(CameraOrientation newOrientation)
    {
        float baseVerticalRotation = 45f;
        float verticalRotationOffset = 90f;
        float numOffsets = 0;

        switch (newOrientation)
        {
            case CameraOrientation.NORTH_WEST:
                cameraOffset = new Vector3(-141.554f, 140f, -141.554f) ;
                baseVerticalRotation = 45f;
                numOffsets = 0;
                break;
            case CameraOrientation.SOUTH_WEST:
                cameraOffset = new Vector3(-141.554f, 140f, +141.554f) ;
                numOffsets = 1;
                break;
            case CameraOrientation.SOUTH_EST:
                cameraOffset = new Vector3(+141.554f, 140f, +141.554f) ;
                numOffsets = 2;
                break;
            case CameraOrientation.NORTH_EST:
                cameraOffset = new Vector3(+141.554f, 140f, -141.554f) ;
                numOffsets = 3;
                break;
            default:
                break;
        }
        cameraContainer.transform.rotation = Quaternion.Euler(cameraContainer.transform.localRotation.eulerAngles.x, baseVerticalRotation + verticalRotationOffset * numOffsets, cameraContainer.transform.localEulerAngles.z);
        _camera_orientation = newOrientation;
    }


    /// <summary>
    /// Sets the new zoom of the camera
    /// </summary>
    /// <param name="newZoom"> A number between 5 (max zoom) and 100 (min zoom) </param>
    public void setCameraZoom(float newZoom)
    {
        if(newZoom > CAMERA_MIN_ZOOM || newZoom < CAMERA_MAX_ZOOM)
        {
            throw new InvalidOperationException("New camera zoom must be a positive float between "+ CAMERA_MAX_ZOOM + " (max zoom) and " + CAMERA_MIN_ZOOM + " (min zoom)!");
        }

        float fov = Mathf.Clamp(newZoom, CAMERA_MAX_ZOOM, CAMERA_MIN_ZOOM);
        Camera.main.orthographicSize = fov;
        _camera_zoom = fov;   
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
        last_state = actual_state;
        actual_state = CameraInteractionState.STOPPED;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - MOUSE_BOUNDS )
        {
            _internalDisplacement = Vector3.forward * Time.deltaTime * (_cameraSpeed + _acceleration);
            cameraContainer.transform.Translate(_internalDisplacement); 
            actual_state = CameraInteractionState.MOVING;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= MOUSE_BOUNDS )
        {
            _internalDisplacement = Vector3.left * Time.deltaTime * (_cameraSpeed + _acceleration);
            cameraContainer.transform.Translate(_internalDisplacement);
            actual_state = CameraInteractionState.MOVING;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= MOUSE_BOUNDS )
        {
            _internalDisplacement = Vector3.back * Time.deltaTime * (_cameraSpeed + _acceleration);
            Vector3 nextFramePosition = cameraContainer.transform.position + _internalDisplacement;
            cameraContainer.transform.Translate(_internalDisplacement);          
            actual_state = CameraInteractionState.MOVING;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - MOUSE_BOUNDS )
        {
            _internalDisplacement = Vector3.right * Time.deltaTime * (_cameraSpeed + _acceleration);
            cameraContainer.transform.Translate(_internalDisplacement); 
            actual_state = CameraInteractionState.MOVING;
        }

        if(actual_state == CameraInteractionState.MOVING)
        {
            _acceleration += BASE_ACCELERATION * Time.deltaTime;
            if (_acceleration > MAX_ACCELERATION) _acceleration = MAX_ACCELERATION;
        }
        else
        {
            _acceleration = 0f;
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