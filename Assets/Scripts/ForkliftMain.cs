using System;
using UnityEngine;

/// <summary>
/// The main class controlling the forklift.
/// 
/// Has a lot of extra code to manage the box's physics, the sole reason
/// being that there is no real friction or joint rigidbodies within the
/// unity engine that would allow me to control everything the way I wanted to.
///
/// Some constraints had led to solutions which disable the following options:
/// - Lifting multiple boxes at the same time.
/// - Totally falling over using the forklift.
/// - The box being lifted is technically "glued" to the fork, while moving.
///
/// Apart from that the class simulates the movement as if it were done using
/// wheels, and also test for collisions on the fork / lifted box.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class ForkliftMain : MonoBehaviour
{
    private static ForkliftMain _instance;
    
    [Header("Properties")] 
    public float motorMaxSpeed = 2.0f;
    public float forkMaxSpeed = 2.0f;
    
    [Header("ROS Controls")]
    public float motorSpeed;
    public float rotationSpeed;
    [Space(2)]
    public Vector2 forkRange = new Vector2(0.0f, 4.0f);
    public float forkSpeed;

    [Header("Fork detector")]
    public Vector3 forkDetectorPos       = Vector3.zero;
    public Vector3 forkDetectorSize      = Vector3.one ;
    public Vector3 forkDetectorOffset    = Vector3.zero;

    public LayerMask forkDetectorLayerMask;
    public int boxLayer = 6;
    public int liftedBoxLayer = 10;
    
    [Header("Fork detector with a box")] 
    public Vector3 forkDetectorBoxSize   = Vector3.one;
    public Vector3 forkDetectorBoxOffset = Vector3.zero;

    [Header("Ground check")]
    public Vector3 groundCheckPos = Vector3.zero;
    public Vector3 groundCheckSize = Vector3.one;
    public LayerMask groundLayer;

    [Header("Box detector")] 
    public Vector3 boxDetectorSize = Vector3.one;
    public Vector3 boxDetectorPos = Vector3.zero;
    public LayerMask boxDetectorMask;
    
    [Header("References")]
    public Transform rotationPoint;
    [Space(2)]
    public GameObject fork;
    public GameObject fakeBoxCollider;
    public Sensor[] sensors;

    private RosManager _manager;

    private Vector3 _forkStartPosition;
    private Vector3 _forkEndPosition;
    private float _forkPos;
    private Rigidbody _rb;

    private Vector3 _lastMoveVector;
    private Transform _hiddenTransform;

    private bool _isLifting;
    private Rigidbody _liftedBox;
    private Vector3 _relativeBoxPos;

    private const int BoxLayer = 6;
    private const int ShelfLayer = 8;

    /// <summary>
    /// The class is managed as a singleton, so when starting it checks if there are multiple instances of it.
    /// </summary>
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Debug.LogError("There are multiple Forklifts found! Check your gameObjects, because this is a singleton!");
            Destroy(this);
        }
    }

    private void Start()
    {
        _manager = RosManager.GetInstance();
        var id = 1;
        foreach (var sensor in sensors)
        {
            sensor.SetId(id);
            _manager.AddSensor(sensor, id);
            id++;
        }
        _forkStartPosition = fork.transform.localPosition;
        _forkEndPosition = _forkStartPosition + new Vector3(0, forkRange.y - forkRange.x, 0);

        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = new Vector3(0.0f, 0.1f, 0.0f);
    }

    private float _angle, _frontalSpeed, _tempPos;
    private Vector3 _newForkPos;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    //
    // ReSharper would like to make this variable local, however I do not want to
    // reallocate the memory every loop.
    private Collider[] _results = new Collider[3];
    private bool _stop;
    private void FixedUpdate()
    {
        _manager.Publish(RosChannels.Publishers.UnityToRosTransform, transform);
        _manager.Publish(RosChannels.Publishers.UnityToRosForkHeight, fork.transform.position.y - _forkStartPosition.y);

        _angle = Mathf.Atan(rotationSpeed);
        _frontalSpeed = 1.0f - Mathf.Abs(rotationSpeed) * 0.66f;

        // Fork hack
        if (_isLifting && _liftedBox.transform.parent == fork.transform)
        {
            var liftedBoxTransform = _liftedBox.transform;
            liftedBoxTransform.localPosition = _relativeBoxPos;
            _liftedBox.velocity = Vector3.zero;
            _liftedBox.angularVelocity = Vector3.zero;
            
            // Physics hackery
            var fakeBoxTransform = fakeBoxCollider.transform;
            fakeBoxTransform.localPosition = liftedBoxTransform.localPosition;
            fakeBoxTransform.rotation = liftedBoxTransform.rotation;
        }

        /* Old movement code not reacting to physics updates */
            //transform.RotateAround(rotationPoint.position, Vector3.up, angle * motorSpeed * motorMaxSpeed);
            //transform.Translate(Vector3.forward * (frontalSpeed * motorSpeed * motorMaxSpeed * Time.fixedDeltaTime), Space.Self);
        /* The fork movement code is still translation based, may pass through shelves. */
            //_forkPos = Mathf.Clamp(_forkPos + forkSpeed * forkMaxSpeed * Time.fixedDeltaTime, forkRange.x, forkRange.y);
            //fork.transform.localPosition = Vector3.Lerp(_forkStartPosition, _forkEndPosition, _forkPos / (forkRange.y - forkRange.x));
            
        // No longer broken, but causes a slight drifting in an unknown direction (around 0.01 per frame while moving)
        // and also the lifting of boxes required some additional workarounds.
        if (OnGround())
        {
            _hiddenTransform = transform;
            _hiddenTransform.RotateAround(rotationPoint.position, Vector3.up, _angle * motorSpeed * motorMaxSpeed);
            if (Mathf.Abs(rotationSpeed) > 0.01f)
                _rb.MoveRotation(_hiddenTransform.rotation);

            _lastMoveVector = transform.localToWorldMatrix.MultiplyVector(Vector3.forward) *
                              (_frontalSpeed * motorSpeed * motorMaxSpeed);
            if (_rb.velocity.magnitude <= motorMaxSpeed)
                _rb.velocity += _lastMoveVector;
            else
                _rb.velocity = (_rb.velocity + _lastMoveVector).normalized * motorMaxSpeed;
        }

        // New fork movement code checking for collisions.
        if (forkSpeed == 0) return;
        
        _tempPos = Mathf.Clamp(_forkPos + forkSpeed * forkMaxSpeed * Time.fixedDeltaTime, forkRange.x, forkRange.y);
        _newForkPos = Vector3.Lerp(_forkStartPosition, _forkEndPosition, _forkPos / (forkRange.y - forkRange.x));
        var boxMode = _isLifting && forkSpeed > 0;
        var count = Physics.OverlapBoxNonAlloc(
            _hiddenTransform.localToWorldMatrix.MultiplyPoint(_newForkPos + forkDetectorPos + (boxMode ? forkDetectorBoxOffset : forkDetectorOffset) * forkSpeed),
            boxMode ? forkDetectorBoxSize : forkDetectorSize, _results, transform.rotation, forkDetectorLayerMask);
        _stop = false;
        for (var i = 0; i < count; i++)
            if (!_results[i].isTrigger)
                _stop = true;

        if (forkSpeed < 0.0f)
        {
            var count2 = Physics.OverlapBoxNonAlloc(
                _hiddenTransform.localToWorldMatrix.MultiplyPoint(_newForkPos + boxDetectorPos),
                boxDetectorSize, _results, transform.rotation, boxDetectorMask);
            for (var i = 0; i < count2; i++)
                if (!_results[i].isTrigger)
                    _stop = true;
        }

        if (_stop) return;
        
        _forkPos = _tempPos;
        fork.transform.localPosition = _newForkPos;
    }

    /// <summary>
    /// I liked to visualise a lot of extra colliders, checks that are not visible while running the simulation.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (rotationPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(rotationPoint.position, 0.05f);
        }

        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_rb.worldCenterOfMass, 0.1f);
        
        if (!Application.isPlaying)
        {
            _forkStartPosition = fork.transform.localPosition;
            _forkEndPosition = _forkStartPosition + new Vector3(0, forkRange.y - forkRange.x, 0);
        }

        Gizmos.color = Color.red;
        var mainTransform = transform;
        var position = mainTransform.position;
        Gizmos.DrawLine(position + new Vector3(0, 0.5f, 0), position + _lastMoveVector.normalized * 2.5f + new Vector3(0, 0.5f, 0));
        
        Gizmos.matrix = mainTransform.localToWorldMatrix;
        Gizmos.DrawCube(_forkStartPosition, Vector3.one * 0.1f);
        Gizmos.DrawCube(_forkEndPosition  , Vector3.one * 0.1f);
        Gizmos.DrawLine(_forkStartPosition      , _forkEndPosition      );

        var forkLocalPos = fork.transform.localPosition;
        Gizmos.DrawWireCube(forkLocalPos + forkDetectorPos + forkDetectorOffset, forkDetectorSize * 2.0f);
        Gizmos.DrawWireCube(forkLocalPos + forkDetectorPos - forkDetectorOffset, forkDetectorSize * 2.0f);
        
        Gizmos.color = new Color(0.33f, 0.0f, 0.66f);
        Gizmos.DrawWireCube(forkLocalPos + forkDetectorPos + forkDetectorBoxOffset, forkDetectorBoxSize * 2.0f);
        Gizmos.DrawWireCube(forkLocalPos + boxDetectorPos, boxDetectorSize * 2.0f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(groundCheckPos, groundCheckSize * 2.0f);
    }

    private bool _justLifted = false; 
    /// <summary>
    /// The function used to set the box being lifted.
    /// If "null" is provided as the box, that counts as putting it down.
    /// </summary>
    /// <param name="obj">The rigidbody of the box being lifted.</param>
    private void SetBox(Rigidbody obj)
    {
        if (_justLifted)
        {
            _justLifted = false;
            return;
        }

        if (_liftedBox != null)
            SetLayerRecursively(_liftedBox.gameObject, boxLayer);    
        
        _isLifting = obj != null;
        _liftedBox = obj;

        if (_isLifting)
        {
            _relativeBoxPos = obj.transform.localPosition;
            SetLayerRecursively(_liftedBox.gameObject, liftedBoxLayer);
            _justLifted = true;
        }

        fakeBoxCollider.SetActive(_isLifting || _justLifted);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.layer)
        {
            case BoxLayer:
                var difference = Vector3.Dot(other.transform.up, Vector3.up);
                if (Math.Abs(difference - 1.0) > 0.1f)
                    return;
                
                other.transform.SetParent(fork.transform);
                SetBox(other.attachedRigidbody);
                break;
            case ShelfLayer:
                other.transform.SetParent(null);
                SetBox(null);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != BoxLayer) return;

        other.transform.SetParent(null);
        SetBox(null);
    }

    /// <summary>
    /// Checks whether the forklift is on the ground.
    /// Mandatory for simulating the wheel movement.
    /// </summary>
    /// <returns>True - if it's on the ground.</returns>
    private bool OnGround()
    {
        var transformMatrix = transform.localToWorldMatrix;
        return Physics.CheckBox(transformMatrix.MultiplyPoint(groundCheckPos), groundCheckSize, transform.rotation, groundLayer);
    }

    public static ForkliftMain GetInstance()
    {
        return _instance;
    }
    
    public void ForkHitGround()
    {
        if (!_isLifting) return;
        
        _liftedBox.transform.SetParent(null);
        SetBox(null);
    }

    private static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }
           
        obj.layer = newLayer;
           
        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}