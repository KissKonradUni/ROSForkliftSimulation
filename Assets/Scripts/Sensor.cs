using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SensorType
{
    distance,
    color
}

public class Sensor : MonoBehaviour
{
    public SensorType type = SensorType.distance;
    public float range = 5.0f;
    public LayerMask layerMask;

    private RosManager _manager;
    private int _id;

    private void Start()
    {
        _manager = RosManager.GetInstance();
    }

    private void FixedUpdate()
    {
        switch (type)
        {
            case SensorType.distance:
                _manager.Publish("sensor" + _id, Random.Range(-1.0f, 5.0f));
                break;
            case SensorType.color:
                _manager.Publish("sensor" + _id, new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void OnDrawGizmos()
    {
        switch (type)
        {
            case SensorType.distance:
                Gizmos.color = Color.green;
                break;
            case SensorType.color:
                Gizmos.color = Color.blue;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var position = transform.position;
        Gizmos.DrawSphere(position, 0.05f);
        Gizmos.DrawLine(position, position + transform.forward * range);
    }
    
    public void SetId(int id)
    {
        _id = id;
    }
}