using System;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

public class MessageInstance
{
    public readonly RosMessageType MessageType;
    public readonly Message Message;
    public readonly Action<Message> ReceiveCallback;

    public MessageInstance(RosMessageType type, Action<Message> receiveCallback = null)
    {
        MessageType = type;
        Message = type switch
        {
            RosMessageType.Float32   => new Float32Msg(),
            RosMessageType.Vector3   => new Message(),
            RosMessageType.Transform => new TransformMsg(),
            _ => null
        };
        ReceiveCallback = receiveCallback;
    }
}