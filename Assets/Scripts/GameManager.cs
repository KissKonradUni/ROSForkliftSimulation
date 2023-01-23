using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool useManualControls;
    public string ip;
    public int port;
    
    private void Awake()
    {   
        DontDestroyOnLoad(this);
    }
}
