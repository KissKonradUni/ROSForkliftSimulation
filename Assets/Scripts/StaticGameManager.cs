using UnityEngine;

public static class StaticGameManager
{
    [Header("Settings")]
    public static bool UseManualControls;
    public static string IP;
    public static int Port;
    public static string Username;
    
    public static void Start()
    {
        var manager = RosManager.GetInstance();
        manager.StartServer();
        
        if (UseManualControls) return;
        manager.testServer.manualDriving = UseManualControls;
        manager.testServer.enabled = false;
        manager.Connect(IP, Port);
    }
}