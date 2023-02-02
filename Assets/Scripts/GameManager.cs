using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Toggle = UnityEngine.UI.Toggle;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    
    [Header("Settings")]
    public bool useManualControls;
    public string ip;
    public int port;
    public string username;

    [Header("References")]
    public TMP_Text playerText;
    public string mainSceneName;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple GameManagers detected! Check your scripts!");
            DestroyImmediate(gameObject);
        }

        DontDestroyOnLoad(this);

        SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            if (newScene.name != mainSceneName)
                return;
            
            StartCoroutine(LateStart());
        };
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.5f);

        var manager = RosManager.GetInstance();
        manager.StartServer();
        
        if (useManualControls) yield break;

        manager.testServer.manualDriving = useManualControls;
        manager.testServer.enabled = false;
        manager.Connect(ip, port);
    }

    public void SetManualControls(Toggle input)
    {
        useManualControls = input.isOn;
    }

    public void SetIP(TMP_InputField input)
    {
        ip = input.text;
    }

    public void SetPort(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out port))
            port = 80;
    }

    public void SetUsername(TMP_InputField input)
    {
        username = (input.text == "") ? "Player" : input.text;
        playerText.text = username;
    }

    public static GameManager GetInstance()
    {
        return _instance;
    }
    
    public void Remove()
    {
        _instance = null;
        Destroy(gameObject);
    }
}
