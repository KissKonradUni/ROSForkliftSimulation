using System;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

public class BaseMessageType
{
    public readonly RosMessageType MessageType;

    protected BaseMessageType(RosMessageType type)
    {
        MessageType = type;
    }
}

public class MessageInstance<T> : BaseMessageType where T : new()
{
    public readonly T Message;
    public readonly Action<T> ReceiveCallback;

    public MessageInstance(RosMessageType type, Action<T> receiveCallback = null) : base(type)
    {
        Message = new T();
        ReceiveCallback = receiveCallback;
    }
}