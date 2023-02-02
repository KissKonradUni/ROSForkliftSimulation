using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")] 
    public WarehouseGenerator warehouseGenerator;
    public TMP_Text shelfSizesText;
    
    [Header("Menus")]
    public RectTransform mainMenu;
    public RectTransform startMenu;
    public RectTransform settingsMenu;
    public RectTransform highscoreMenu;

    [Header("Others")] 
    public TMP_Text highscoreNames;
    public TMP_Text highscoreScores;
    public Animator blackout;
    public string mainScene;

    public void StartGame()
    {
        StartCoroutine(LateStartGame());
    }

    private IEnumerator LateStartGame()
    {
        blackout.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(mainScene);
    }
    
    public void StartButton()
    {
        mainMenu.localPosition = new Vector3(0, -10000);
        startMenu.localPosition = new Vector3(0, 0);
    }

    public void SettingsButton()
    {
        mainMenu.localPosition = new Vector3(0, -10000);
        settingsMenu.localPosition = new Vector3(2.5f, 0);
    }

    public void HighscoreButton()
    {
        HighscoreManager.Load();
        HighscoreManager.Highscores.Sort((a, b) => a.points.CompareTo(b.points));
        HighscoreManager.Highscores.Reverse();
        var names  = HighscoreManager.Highscores.Aggregate("", (current, highscore) => current + $"{highscore.name}\n"  );
        var scores = HighscoreManager.Highscores.Aggregate("", (current, highscore) => current + $"{highscore.points}\n");
        highscoreNames.text  = names ;
        highscoreScores.text = scores;
        
        mainMenu.localPosition = new Vector3(0, -10000);
        highscoreMenu.localPosition = new Vector3(0, 0);
    }

    public void ExitButton()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
        }
        else
        {
            Application.Quit();
        }
    }
    
    public void BackButton()
    {
        settingsMenu.localPosition = new Vector3(0, -10000);
        startMenu.localPosition = new Vector3(0, -10000);
        highscoreMenu.localPosition = new Vector3(0, -10000);
        mainMenu.localPosition = new Vector3(0, 0);
    }

    public void SeedSet(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out warehouseGenerator.seed))
            warehouseGenerator.seed = -1;
    }

    public void JobTypeSet(TMP_Dropdown input)
    {
        warehouseGenerator.jobType = (JobType)input.value;
    }
    
    public void RequirementSet(TMP_InputField input)
    {
        warehouseGenerator.searchPercent = int.TryParse(input.text, out warehouseGenerator.searchPercent) ? 
            Mathf.Clamp(warehouseGenerator.searchPercent, 20, 100) : 50;
    }

    public void HoursSet(TMP_InputField input)
    {
        TimeLimitSet(0, 0, input);
    }
    
    public void MinuteSet(TMP_InputField input)
    {
        TimeLimitSet(1, 15, input);
    }
    
    public void SecondSet(TMP_InputField input)
    {
        TimeLimitSet(2, 0, input);
    }

    private void TimeLimitSet(int pos, float @default, TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = @default;
        val = Mathf.Clamp(val, 0.0f, 60.0f);
        warehouseGenerator.timeLimit[pos] = val;
    }

    public void SetWarehouseX(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 24;
        val = Mathf.Clamp(val, 8, 100);
        warehouseGenerator.buildingSize.x = val;
        
        CalculateDistance();
    }

    public void SetWarehouseY(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 44;
        val = Mathf.Clamp(val, 8, 100);
        warehouseGenerator.buildingSize.y = val;
        
        CalculateDistance();
    }

    public void SetShelfX(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 2;
        val = Mathf.Clamp(val, 1, 100);
        warehouseGenerator.shelfColumns = val;
        
        CalculateDistance();
    }

    public void SetShelfY(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 4;
        val = Mathf.Clamp(val, 1, 100);
        warehouseGenerator.shelfRows = val;
        
        CalculateDistance();
    }

    private void CalculateDistance()
    {
        warehouseGenerator.distanceBetweenShelves = (warehouseGenerator.buildingSize.y - warehouseGenerator.shelfRows) / (warehouseGenerator.shelfRows + 1);
        warehouseGenerator.distanceToWall = (warehouseGenerator.buildingSize.x - warehouseGenerator.shelfColumns * 4.0f) / 2.0f;
        shelfSizesText.text = $"{warehouseGenerator.distanceBetweenShelves:0.00} m\n{warehouseGenerator.distanceToWall:0.00} m";
    }

    public void SetToggleBlue(Toggle input)
    {
        warehouseGenerator.boxTypes[0] = input.isOn;
    }
    
    public void SetToggleGreen(Toggle input)
    {
        warehouseGenerator.boxTypes[1] = input.isOn;
    }
    
    public void SetToggleYellow(Toggle input)
    {
        warehouseGenerator.boxTypes[2] = input.isOn;
    }
    
    public void SetToggleRed(Toggle input)
    {
        warehouseGenerator.boxTypes[3] = input.isOn;
    }

    public void SetWeightBlue(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        warehouseGenerator.boxWeights[0] = val;
    }
    
    public void SetWeightGreen(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        warehouseGenerator.boxWeights[1] = val;
    }
    
    public void SetWeightYellow(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        warehouseGenerator.boxWeights[2] = val;
    }
    
    public void SetWeightRed(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        warehouseGenerator.boxWeights[3] = val;
    }

    public void SetZoneMixed(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        warehouseGenerator.zoneSizes[0] = val;
    }
    
    public void SetZoneBlue(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        warehouseGenerator.zoneSizes[1] = val;
    }
    
    public void SetZoneGreen(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        warehouseGenerator.zoneSizes[2] = val;
    }
    
    public void SetZoneYellow(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        warehouseGenerator.zoneSizes[3] = val;
    }
    
    public void SetZoneRed(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        warehouseGenerator.zoneSizes[4] = val;
    }
}
