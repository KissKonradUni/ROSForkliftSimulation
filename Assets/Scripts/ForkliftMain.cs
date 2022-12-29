using System;
using UnityEngine;

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
    public Vector3 forkDetectorPos    = Vector3.zero;
    public Vector3 forkDetectorSize   = Vector3.one ;
    public Vector3 forkDetectorOffset = Vector3.zero;
    public LayerMask forkDetectorLayerMask;
    
    [Header("Ground check")]
    public Vector3 groundCheckPos = Vector3.zero;
    public Vector3 groundCheckSize = Vector3.one;
    public LayerMask groundLayer;
    
    [Header("References")]
    public Transform rotationPoint;
    [Space(2)]
    public GameObject fork;
    public Sensor[] sensors;

    private RosManager _manager;

    private Vector3 _forkStartPosition;
    private Vector3 _forkEndPosition;
    private float _forkPos;
    private RaycastHit _hitInfo;
    private Rigidbody _rb;

    private Vector3 _lastMoveVector;
    private Transform _hiddenTransform;

    private bool _isLifting;
    private Rigidbody _liftedBox;
    private Vector3 _relativeBoxPos;

    private const int BoxLayer = 6;
    private const int ShelfLayer = 8;

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
    private Collider[] _results = new Collider[3];
    private bool _stop;
    private void FixedUpdate()
    {
        _manager.Publish(RosChannels.Publishers.UnityToRosTransform, transform);
        _manager.Publish(RosChannels.Publishers.UnityToRosForkHeight, fork.transform.position.y - _forkStartPosition.y);

        _angle = Mathf.Atan(rotationSpeed);
        _frontalSpeed = 1.0f - Mathf.Abs(rotationSpeed) * 0.66f;

        // Fork hack
        if (_isLifting)
        {
            _liftedBox.transform.localPosition = _relativeBoxPos;
            _liftedBox.velocity = Vector3.zero;
        }

        /* Old movement code not reacting to physics updates */
//        transform.RotateAround(rotationPoint.position, Vector3.up, angle * motorSpeed * motorMaxSpeed);
//        transform.Translate(Vector3.forward * (frontalSpeed * motorSpeed * motorMaxSpeed * Time.fixedDeltaTime), Space.Self);
        /* The fork movement code is still translation based, may pass through shelves. */
//        _forkPos = Mathf.Clamp(_forkPos + forkSpeed * forkMaxSpeed * Time.fixedDeltaTime, forkRange.x, forkRange.y);
//        fork.transform.localPosition = Vector3.Lerp(_forkStartPosition, _forkEndPosition, _forkPos / (forkRange.y - forkRange.x));

        /* New moment code, that breaks the fork/box interactions sadly */
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
        var count = Physics.OverlapBoxNonAlloc(_hiddenTransform.localToWorldMatrix.MultiplyPoint(_newForkPos + forkDetectorPos + forkDetectorOffset * forkSpeed), forkDetectorSize, _results, transform.rotation, forkDetectorLayerMask);
        _stop = false;
        for (var i = 0; i < count; i++)
            if (!_results[i].isTrigger)
                _stop = true;
        
        if (_stop) return;
        
        _forkPos = _tempPos;
        fork.transform.localPosition = _newForkPos;
    }

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
        
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(groundCheckPos, groundCheckSize * 2.0f);
    }

    private void SetBox(Rigidbody obj)
    {
        _isLifting = obj != null;
        _liftedBox = obj;

        if (_isLifting)
            _relativeBoxPos = obj.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.layer)
        {
            case BoxLayer:
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
}
