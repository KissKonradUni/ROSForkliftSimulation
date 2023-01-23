using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public RectTransform MainMenu;
    public RectTransform StartMenu;
    public RectTransform SettingsMenu;
    public RectTransform HighscoreMenu;
    
    public void StartButton()
    {
        MainMenu.localPosition = new Vector3(0, -10000);
        StartMenu.localPosition = new Vector3(0, 0);
    }

    public void SettingsButton()
    {
        MainMenu.localPosition = new Vector3(0, -10000);
        SettingsMenu.localPosition = new Vector3(2.5f, 0);
    }

    public void BackButton()
    {
        SettingsMenu.localPosition = new Vector3(0, -10000);
        MainMenu.localPosition = new Vector3(0, 0);
    }
}
