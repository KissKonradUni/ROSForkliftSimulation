using UnityEngine;

public static class StaticGameManager
{
    [Header("Settings")]
    public static bool UseManualControls = true;
    public static string IP = "127.0.0.1";
    public static int Port = 80;
    public static string Username = "Player";
    
    public static void Start()
    {
        var manager = RosManager.GetInstance();
        manager.testServer.enabled = false;
        manager.StartServer(UseManualControls);
        
        if (UseManualControls) return;
        manager.testServer.manualDriving = UseManualControls;
        manager.Connect(IP, Port);
    }
}