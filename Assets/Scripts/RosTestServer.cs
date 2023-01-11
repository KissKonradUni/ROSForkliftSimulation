using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is purely used to test features in the editor.
/// Shouldn't be used outside the editor environment.
///
/// (This "mock" server allows the user to control the motors using wasd controls, and qe for lifting the fork itself.)
/// </summary>
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