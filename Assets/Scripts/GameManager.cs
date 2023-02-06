using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TMP_Text usernameText;
    public Toggle manualControlToggle;
    public TMP_InputField ipText;
    public TMP_InputField portText;

    private void Start()
    {
        usernameText.text = StaticGameManager.Username;
        manualControlToggle.isOn = StaticGameManager.UseManualControls;
        ipText.text = StaticGameManager.IP;
        portText.text = StaticGameManager.Port.ToString();
    }

    public void SetManualControls(Toggle input)
    {
        StaticGameManager.UseManualControls = input.isOn;
    }

    public void SetIP(TMP_InputField input)
    {
        StaticGameManager.IP = input.text;
    }

    public void SetPort(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out StaticGameManager.Port))
            StaticGameManager.Port = 80;
    }

    public void SetUsername(TMP_InputField input)
    {
        StaticGameManager.Username = (input.text == "") ? "Player" : input.text;
    }

    public void SetPlayerText(TMP_Text text)
    {
        text.text = StaticGameManager.Username;
    }
}
