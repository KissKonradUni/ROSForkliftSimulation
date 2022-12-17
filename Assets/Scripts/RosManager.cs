using System;
using System.Collections.Generic;
using System.Globalization;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

#region Enums

public enum RosMessageType
{
    Float32,
    Vector3,
    Transform
}

public enum RosDirection
{
    Listener,
    Publisher
}

#endregion

namespace RosChannels
{
    public static class Publishers
    {
        public const string UnityToRosTransform  = "unityToRosTransform" ;
        public const string UnityToRosForkHeight = "unityToRosForkHeight";
    }

    public static class Listeners
    {
        public const string RosToUnityMotorSpeed    = "rosToUnityMotorSpeed"   ;
        public const string RosToUnityRotationSpeed = "rosToUnityRotationSpeed";
        public const string RosToUnityForkSpeed     = "rosToUnityForkSpeed"    ;
    }
}

public class RosManager : MonoBehaviour
{
    private static RosManager _instance;
    private static ForkliftMain _forklift;
    
    private ROSConnection _connection;

    public RosTestServer testServer;
    private bool _testMode;
    private static readonly Dictionary<string, MessageInstance> RosPublishers = new()
    {
        {RosChannels.Publishers.UnityToRosTransform,  new MessageInstance(RosMessageType.Transform)},
        {RosChannels.Publishers.UnityToRosForkHeight, new MessageInstance(RosMessageType.Float32  )}
    };
    private static readonly Dictionary<string, MessageInstance> RosListeners = new()
    {
        {RosChannels.Listeners.RosToUnityMotorSpeed, new MessageInstance(RosMessageType.Float32, msg => {
            _forklift.motorSpeed = Mathf.Clamp(((Float32Msg)msg).data, -1.0f, 1.0f);
        })},
        {RosChannels.Listeners.RosToUnityRotationSpeed, new MessageInstance(RosMessageType.Float32, msg => {
            _forklift.rotationSpeed = Mathf.Clamp(((Float32Msg)msg).data, -1.0f, 1.0f);
        })},
        {RosChannels.Listeners.RosToUnityForkSpeed, new MessageInstance(RosMessageType.Float32, msg => {
            _forklift.forkSpeed = Mathf.Clamp(((Float32Msg)msg).data, -1.0f, 1.0f);
        })},
    };

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Debug.LogError("There are multiple ROS Managers found! Check your gameObjects, because this is a singleton!");
            Destroy(this);
            return;
        }

        var forkliftObject = GameObject.FindWithTag("Forklift");
        _forklift = forkliftObject != null ? forkliftObject.GetComponent<ForkliftMain>() : null;
        if (_forklift == null)
        {
            Debug.LogError("No forklift found! Halting ROS Manager.");
            gameObject.SetActive(false);
            return;
        }
        
        _connection = ROSConnection.GetOrCreateInstance();
        RosSetupPublishers();
        RosSetupListeners();
    }

    private void Start()
    {
        if (testServer == null || !testServer.isActiveAndEnabled) return;
        
        _testMode = true;
        SetupTestServer();
    }

    public static RosManager GetInstance()
    {
        return _instance;
    }

    #region Test Server

    private void SetupTestServer()
    {
        foreach (var publisher in RosPublishers)
        {
            testServer.Listeners[publisher.Key] = "-";
        }

        foreach (var listener in RosListeners)
        {
            testServer.Publishers[listener.Key] = listener.Value.MessageType;
        }
    }

    public void TestReceiveMessage(string channel, float value)
    {
        var listener = RosListeners[channel];
        if (listener == null)
        {
            Debug.LogError($"Channel with name \"{channel}\" not found.");
            return;
        }
            
        if (listener.MessageType != RosMessageType.Float32)
        {
            Debug.LogError($"Invalid type of message received on channel \"{channel}\".");
            return;
        }

        listener.ReceiveCallback(new Float32Msg(value));
    }
    
    public void TestReceiveMessage(string channel, Vector3 value)
    {
        var listener = RosListeners[channel];
        if (listener == null)
        {
            Debug.LogError($"Channel with name \"{channel}\" not found.");
            return;
        }
            
        if (listener.MessageType != RosMessageType.Float32)
        {
            Debug.LogError($"Invalid type of message received on channel \"{channel}\".");
            return;
        }

        listener.ReceiveCallback(new Vector3Msg(value.x, value.y, value.z));
    }
    
    public void TestReceiveMessage(string channel, Transform value)
    {
        var listener = RosListeners[channel];
        if (listener == null)
        {
            Debug.LogError($"Channel with name \"{channel}\" not found.");
            return;
        }
            
        if (listener.MessageType != RosMessageType.Float32)
        {
            Debug.LogError($"Invalid type of message received on channel \"{channel}\".");
            return;
        }

        var position = value.position;
        var rotation = value.rotation;
        listener.ReceiveCallback(new TransformMsg(
            new Vector3Msg(position.x, position.y, position.z), 
            new QuaternionMsg(rotation.x, rotation.y, rotation.z, rotation.w)
        ));
    }

    #endregion

    #region Ros Abstraction

    private void RosSetupPublishers()
    {
        foreach (var channel in RosPublishers)
        {
            switch (channel.Value.MessageType)
            {
                case RosMessageType.Float32:
                    _connection.RegisterPublisher<Float32Msg>(channel.Key);
                    break;
                case RosMessageType.Vector3:
                    _connection.RegisterPublisher<Vector3Msg>(channel.Key);
                    break;
                case RosMessageType.Transform:
                    _connection.RegisterPublisher<TransformMsg>(channel.Key);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void RosSetupListeners()
    {
        foreach (var channel in RosListeners)
        {
            _connection.Subscribe(channel.Key, channel.Value.ReceiveCallback);
        }
    }
    
    public void Publish(string messageName, float value)
    {
        var instance = RosPublishers[messageName];
        if (ReferenceEquals(instance, null))
        {
            Debug.LogError($"The publisher named \"{messageName}\" was not found!");
            return;
        }
        
        if (instance.MessageType == RosMessageType.Float32)
        {
            if (_testMode)
            {
                testServer.Listeners[messageName] = value.ToString("F3", CultureInfo.CurrentCulture);
                return;
            }
            
            var msg = instance.Message;
            ((Float32Msg)msg).data = value;
            _connection.Publish(messageName, instance.Message);
        }
        else
            throw new InvalidOperationException();
    }
    
    public void Publish(string messageName, Vector3 value)
    {
        var instance = RosPublishers[messageName];
        if (ReferenceEquals(instance, null))
        {
            Debug.LogError($"The publisher named \"{messageName}\" was not found!");
            return;
        }
        
        if (instance.MessageType == RosMessageType.Vector3)
        {
            if (_testMode)
            {
                if (messageName.Contains("sensor"))
                {
                    testServer.Listeners[messageName] = $"C {value.x} {value.y} {value.z}";    
                } else
                    testServer.Listeners[messageName] = value.ToString("F3");
                return;
            }
            
            var msg = instance.Message;
            var vector3Msg = ((Vector3Msg)msg);
            vector3Msg.x = value.x;
            vector3Msg.y = value.y;
            vector3Msg.z = value.z;
            _connection.Publish(messageName, instance.Message);
        }
        else
            throw new InvalidOperationException();
    }
    
    public void Publish(string messageName, Transform value)
    {
        var instance = RosPublishers[messageName];
        if (ReferenceEquals(instance, null))
        {
            Debug.LogError($"The publisher named \"{messageName}\" was not found!");
            return;
        }
        
        if (instance.MessageType == RosMessageType.Transform)
        {
            if (_testMode)
            {
                testServer.Listeners[messageName] = value.position.ToString("F3") + "|" + value.rotation.ToString("F3");
                return;
            }
            
            var msg = instance.Message;
            var transformMsg = ((TransformMsg)msg);
            var position = value.position;
            var rotation = value.rotation;
            transformMsg.translation.x = position.x;
            transformMsg.translation.y = position.y;
            transformMsg.translation.z = position.z;
            transformMsg.rotation.x = rotation.x;
            transformMsg.rotation.y = rotation.y;
            transformMsg.rotation.z = rotation.z;
            transformMsg.rotation.w = rotation.w;
            _connection.Publish(messageName, instance.Message);
        }
        else
            throw new InvalidOperationException();
    }

    #endregion

    public void AddSensor(Sensor sensor, int id)
    {
        switch (sensor.type)
        {
            case SensorType.Distance:
                RosPublishers.Add("sensor" + id, new MessageInstance(RosMessageType.Float32));
                _connection.RegisterPublisher<Float32Msg>("sensor" + id);
                break;
            case SensorType.Color:
                RosPublishers.Add("sensor" + id, new MessageInstance(RosMessageType.Vector3));
                _connection.RegisterPublisher<Vector3Msg>("sensor" + id);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}