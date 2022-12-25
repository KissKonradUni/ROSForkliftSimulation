using System.Collections.Generic;
using UnityEngine;

public class RosTestServer : MonoBehaviour
{
    public readonly Dictionary<string, string> Listeners = new();
    public readonly Dictionary<string, RosMessageType> Publishers = new();
    public bool manualDriving;

    public Vector3 userInput;
    private RosManager _manager;

    private void Start()
    {
        _manager = RosManager.GetInstance();
    }

    private void Update()
    {
        if (!manualDriving)
            return;
        
        userInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetAxis("Fork"));
    }

    private void FixedUpdate()
    {
        if (!manualDriving)
            return;

        _manager.TestReceiveMessage(RosChannels.Listeners.RosToUnityRotationSpeed, userInput.x);
        _manager.TestReceiveMessage(RosChannels.Listeners.RosToUnityMotorSpeed   , userInput.y);
        _manager.TestReceiveMessage(RosChannels.Listeners.RosToUnityForkSpeed    , userInput.z);
    }
}