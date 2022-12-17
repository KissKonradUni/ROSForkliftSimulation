using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SensorType
{
    Distance,
    Color
}

public class Sensor : MonoBehaviour
{
    public SensorType type = SensorType.Distance;
    public float range = 5.0f;
    public LayerMask layerMask;

    private RosManager _manager;
    private int _id;

    private RenderTexture _localTexture;
    private Texture2D _texture2D;
    private Camera _camera;

    private Color _colorValue;
    
    private void Start()
    {
        _manager = RosManager.GetInstance();

        if (type != SensorType.Color) return;
        
        _localTexture = new RenderTexture(3, 3, 24);
        _localTexture.Create();
        _texture2D = new Texture2D(3, 3);
        
        var cameraGameObject = new GameObject("Camera");
        cameraGameObject.transform.SetParent(transform);
        cameraGameObject.transform.localPosition = Vector3.zero;
        cameraGameObject.transform.rotation = Quaternion.identity;
        _camera = cameraGameObject.AddComponent<Camera>();
        _camera.backgroundColor = Color.clear;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.farClipPlane = range;
        _camera.targetTexture = _localTexture;
        _camera.cullingMask = layerMask;
    }

    private void FixedUpdate()
    {
        switch (type)
        {
            case SensorType.Distance:
                RaycastHit hitInfo;
                var trans = transform;
                var success = Physics.Raycast(trans.position, trans.forward, out hitInfo, range, layerMask);
                _manager.Publish("sensor" + _id, success ? hitInfo.distance : -1.0f);
                break;
            case SensorType.Color:
                _manager.Publish("sensor" + _id, new Vector3(_colorValue.r, _colorValue.g, _colorValue.b));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void LateUpdate()
    {
        if (type == SensorType.Color)
        {
            RenderTexture.active = _localTexture;
            _texture2D.ReadPixels(new Rect(1, 1, 1, 1), 1, 1);
            _texture2D.Apply();
            _colorValue = _texture2D.GetPixel(1, 1);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = type switch
        {
            SensorType.Distance => Color.green,
            SensorType.Color    => Color.blue,
            _ => throw new ArgumentOutOfRangeException()
        };

        var position = transform.position;
        Gizmos.DrawSphere(position, 0.05f);
        Gizmos.DrawLine(position, position + transform.forward * range);
    }
    
    public void SetId(int id)
    {
        _id = id;
    }
}