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

#endregion

namespace RosChannels
{
    /// <summary>
    /// The main publishers used by the unity ros server during communication.
    /// </summary>
    public static class Publishers
    {
        public const string UnityToRosTransform  = "unityToRosTransform" ;
        public const string UnityToRosForkHeight = "unityToRosForkHeight";
    }

    /// <summary>
    /// The main listeners used by the unity ros server during communication.
    /// </summary>
    public static class Listeners
    {
        public const string RosToUnityMotorSpeed    = "rosToUnityMotorSpeed"   ;
        public const string RosToUnityRotationSpeed = "rosToUnityRotationSpeed";
        public const string RosToUnityForkSpeed     = "rosToUnityForkSpeed"    ;
    }
}

/// <summary>
/// The main class used to communicate with server using the ROSConnection protocol.
///
/// Also allows the use of the "RosTestServer" class, allowing to test features within the editor.
/// 
/// The class is used as a singleton.
/// </summary>
public class RosManager : MonoBehaviour
{
    private static RosManager _instance;
    private static ForkliftMain _forklift;
    
    private ROSConnection _connection;

    public RosTestServer testServer;
    private bool _testMode = false;
    
    /// <summary>
    /// The main dictionary that lists the Publishers going form the unity ros server to the real one.
    /// This list is expanded upon at runtime, by adding the sensors to it.
    /// It's main purpose is to define what type each channel is, and allows an easy was to add more channels. 
    /// </summary>
    private readonly Dictionary<string, BaseMessageType> _rosPublishers = new()
    {
        {RosChannels.Publishers.UnityToRosTransform,  new MessageInstance<TransformMsg>(RosMessageType.Transform)},
        {RosChannels.Publishers.UnityToRosForkHeight, new MessageInstance<Float32Msg>  (RosMessageType.Float32  )}
    };
    
    /// <summary>
    /// The main dictionary that lists the Listeners coming from the real ros server to the unity one.
    /// It's main purpose is to define what type each channel is, and to define the callbacks using the received information.
    /// It also provides an easy way to add additional channels.
    /// </summary>
    private readonly Dictionary<string, BaseMessageType> _rosListeners = new()
    {
        {RosChannels.Listeners.RosToUnityMotorSpeed, new MessageInstance<Float32Msg>(RosMessageType.Float32, msg => {
            _forklift.motorSpeed = Mathf.Clamp(msg.data, -1.0f, 1.0f);
        })},
        {RosChannels.Listeners.RosToUnityRotationSpeed, new MessageInstance<Float32Msg>(RosMessageType.Float32, msg => {
            _forklift.rotationSpeed = Mathf.Clamp(msg.data, -1.0f, 1.0f);
        })},
        {RosChannels.Listeners.RosToUnityForkSpeed, new MessageInstance<Float32Msg>(RosMessageType.Float32, msg => {
            _forklift.forkSpeed = Mathf.Clamp(msg.data, -1.0f, 1.0f);
        })},
    };

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Debug.LogWarning("There are multiple ROS Managers found! Check your gameObjects, because this is a singleton!");
            DestroyImmediate(gameObject);
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

    public void StartServer(bool useManualControls)
    {
        if (testServer == null || !testServer.isActiveAndEnabled)
        {
            
        }
        else
        {
            _testMode = useManualControls;
            SetupTestServer();
        }
    }

    public void Connect(string ip, int port)
    {
        _connection.RosIPAddress = ip;
        _connection.RosPort = port;
        _connection.Connect();
    }
    
    public void Disconnect()
    {
        _connection.Disconnect();
        testServer.enabled = false;
        _forklift.Stop();
    }

    public static RosManager GetInstance()
    {
        return _instance;
    }

    #region Test Server

    private void SetupTestServer()
    {
        foreach (var publisher in _rosPublishers)
        {
            testServer.Listeners[publisher.Key] = "-";
        }

        foreach (var listener in _rosListeners)
        {
            testServer.Publishers[listener.Key] = listener.Value.MessageType;
        }
    }

    public void TestReceiveMessage(string channel, float value)
    {
        var listener = _rosListeners[channel];
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

        ((MessageInstance<Float32Msg>)listener).ReceiveCallback(new Float32Msg(value));
    }
    
    public void TestReceiveMessage(string channel, Vector3 value)
    {
        var listener = _rosListeners[channel];
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

        ((MessageInstance<Vector3Msg>)listener).ReceiveCallback(new Vector3Msg(value.x, value.y, value.z));
    }
    
    public void TestReceiveMessage(string channel, Transform value)
    {
        var listener = _rosListeners[channel];
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
        ((MessageInstance<TransformMsg>)listener).ReceiveCallback(new TransformMsg(
            new Vector3Msg(position.x, position.y, position.z), 
            new QuaternionMsg(rotation.x, rotation.y, rotation.z, rotation.w)
        ));
    }

    #endregion

    #region Ros Abstraction

    private void RosSetupPublishers()
    {
        foreach (var channel in _rosPublishers)
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
        foreach (var channel in _rosListeners)
        {
            switch (channel.Value.MessageType)
            {
                case RosMessageType.Float32:
                    _connection.Subscribe<Float32Msg>(channel.Key, ((MessageInstance<Float32Msg>)channel.Value).ReceiveCallback);
                    break;
                case RosMessageType.Vector3:
                    _connection.Subscribe<Vector3Msg>(channel.Key, ((MessageInstance<Vector3Msg>)channel.Value).ReceiveCallback);
                    break;
                case RosMessageType.Transform:
                    _connection.Subscribe<TransformMsg>(channel.Key, ((MessageInstance<TransformMsg>)channel.Value).ReceiveCallback);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    /// <summary>
    /// Publishes the message to the real server from unity.
    /// When using it with a test server, it just passes the information to there.
    /// </summary>
    /// <param name="messageName">The name of the channel used.</param>
    /// <param name="value">The float32 value being passed.</param>
    /// <exception cref="InvalidOperationException">If type of the channel is different, than the value trying to be sent, this exception is thrown.</exception>
    public void Publish(string messageName, float value)
    {
        var instance = _rosPublishers[messageName];
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

            var inst = (MessageInstance<Float32Msg>)instance; 
            var msg = inst.Message;
            msg.data = value;
            _connection.Publish(messageName, msg);
        }
        else
            throw new InvalidOperationException();
    }
    
    /// <summary>
    /// Publishes the message to the real server from unity.
    /// When using it with a test server, it just passes the information to there.
    /// </summary>
    /// <param name="messageName">The name of the channel used.</param>
    /// <param name="value">The vector3 value being passed.</param>
    /// <exception cref="InvalidOperationException">If type of the channel is different, than the value trying to be sent, this exception is thrown.</exception>
    public void Publish(string messageName, Vector3 value)
    {
        var instance = _rosPublishers[messageName];
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
            
            var inst = (MessageInstance<Vector3Msg>)instance; 
            var vector3Msg = inst.Message;
            vector3Msg.x = value.x;
            vector3Msg.y = value.y;
            vector3Msg.z = value.z;
            _connection.Publish(messageName, vector3Msg);
        }
        else
            throw new InvalidOperationException();
    }
    
    /// <summary>
    /// Publishes the message to the real server from unity.
    /// When using it with a test server, it just passes the information to there.
    /// It also converts the unity transform to ros transform before being passed.
    /// </summary>
    /// <param name="messageName">The name of the channel used.</param>
    /// <param name="value">The transform value being passed.</param>
    /// <exception cref="InvalidOperationException">If type of the channel is different, than the value trying to be sent, this exception is thrown.</exception>
    public void Publish(string messageName, Transform value)
    {
        var instance = _rosPublishers[messageName];
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
            
            var inst = (MessageInstance<TransformMsg>)instance; 
            var transformMsg = inst.Message;
            var position = value.position;
            var rotation = value.rotation;
            transformMsg.translation.x = position.x;
            transformMsg.translation.y = position.y;
            transformMsg.translation.z = position.z;
            transformMsg.rotation.x = rotation.x;
            transformMsg.rotation.y = rotation.y;
            transformMsg.rotation.z = rotation.z;
            transformMsg.rotation.w = rotation.w;
            _connection.Publish(messageName, transformMsg);
        }
        else
            throw new InvalidOperationException();
    }

    #endregion

    
    /// <summary>
    /// Allows the addition of sensors to the channel list.
    /// Usually called during runtime.
    /// </summary>
    /// <param name="sensor">The sensor itself being used</param>
    /// <param name="id">The id of the sensor starting from 0</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the Sensor's type being specified cannot be handled.</exception>
    public void AddSensor(Sensor sensor, int id)
    {
        switch (sensor.type)
        {
            case SensorType.Distance:
                _rosPublishers.Add("sensor" + id, new MessageInstance<Float32Msg>(RosMessageType.Float32));
                _connection.RegisterPublisher<Float32Msg>("sensor" + id);
                break;
            case SensorType.Color:
                _rosPublishers.Add("sensor" + id, new MessageInstance<Vector3Msg>(RosMessageType.Vector3));
                _connection.RegisterPublisher<Vector3Msg>("sensor" + id);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Remove()
    {
        _instance = null;
        Destroy(gameObject);
    }
}