using UnityEngine;

public class Blackout : MonoBehaviour
{
    private static Blackout _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Debug.LogWarning("Multiple blackouts found! Check your scripts!");
            DestroyImmediate(gameObject);
        }
        
        DontDestroyOnLoad(this);
    }

    public static Blackout GetInstance()
    {
        return _instance;
    }
    
    public void Remove()
    {
        _instance = null;
        Destroy(gameObject);
    }
}
