using UnityEngine;

[SelectionBase]
public class ForkliftMain : MonoBehaviour
{
    [Header("Properties")] 
    public float motorMaxSpeed = 2.0f;
    public float forkMaxSpeed = 2.0f;
    
    [Header("ROS Controls")]
    public float motorSpeed;
    public float rotationSpeed;
    [Space(2)]
    public Vector2 forkRange = new Vector2(0.0f, 4.0f);
    public float forkSpeed;

    [Header("References")]
    public Transform rotationPoint;
    [Space(2)]
    public GameObject fork;
    public Sensor[] sensors;
    
    private RosManager _manager;

    private Vector3 _forkStartPosition;
    private Vector3 _forkEndPosition;
    private float _forkPos;

    void Start()
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
    }
    
    void FixedUpdate()
    {
        _manager.Publish(RosChannels.Publishers.UnityToRosTransform, transform);
        _manager.Publish(RosChannels.Publishers.UnityToRosForkHeight, fork.transform.position.y - _forkStartPosition.y);

        var sideDifference = rotationSpeed * motorSpeed;
        transform.RotateAround(rotationPoint.position, Vector3.up, sideDifference);
        transform.Translate(Vector3.forward * (motorSpeed * motorMaxSpeed * Time.fixedDeltaTime), Space.Self);

        _forkPos = Mathf.Clamp(_forkPos + forkSpeed * forkMaxSpeed * Time.fixedDeltaTime, forkRange.x, forkRange.y);
        var position = transform.position;
        fork.transform.localPosition = Vector3.Lerp(_forkStartPosition, _forkEndPosition, _forkPos / (forkRange.y - forkRange.x));
    }

    private void OnDrawGizmos()
    {
        if (rotationPoint == null) return;
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(rotationPoint.position, 0.05f);

        if (!Application.isPlaying)
        {
            _forkStartPosition = fork.transform.position;
            _forkEndPosition = _forkStartPosition + new Vector3(0, forkRange.y - forkRange.x, 0);
        }

        Gizmos.color = Color.red;
        var position = transform.position;
        Gizmos.DrawCube(position + _forkStartPosition, Vector3.one * 0.1f);
        Gizmos.DrawCube(position + _forkEndPosition  , Vector3.one * 0.1f);
        Gizmos.DrawLine(position + _forkStartPosition, position + _forkEndPosition);
    }
}
