using System.Collections.Generic;
using UnityEngine;

public class RosTestServer : MonoBehaviour
{
    public readonly Dictionary<string, string> Listeners = new();
    public readonly Dictionary<string, RosMessageType> Publishers = new();
}