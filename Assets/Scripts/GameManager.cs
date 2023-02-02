using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
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
