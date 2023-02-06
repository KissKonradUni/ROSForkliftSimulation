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
    
    private static readonly int FadeIn = Animator.StringToHash("FadeIn");

    public void StartGame()
    {
        blackout = Blackout.GetInstance().GetComponentInChildren<Animator>();
        StartCoroutine(LateStartGame());
    }

    private IEnumerator LateStartGame()
    {
        blackout.SetTrigger(FadeIn);
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
        var names  = HighscoreManager.Highscores.Take(10).Aggregate("", (current, highscore) => current + $"{highscore.name}\n"  );
        var scores = HighscoreManager.Highscores.Take(10).Aggregate("", (current, highscore) => current + $"{highscore.points}\n");
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
        if (!int.TryParse(input.text, out WarehouseGenerator.Seed))
            WarehouseGenerator.Seed = -1;
    }

    public void JobTypeSet(TMP_Dropdown input)
    {
        WarehouseGenerator.JobType = (JobType)input.value;
    }
    
    public void RequirementSet(TMP_InputField input)
    {
        WarehouseGenerator.SearchPercent = int.TryParse(input.text, out WarehouseGenerator.SearchPercent) ? 
            Mathf.Clamp(WarehouseGenerator.SearchPercent, 20, 100) : 50;
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
        WarehouseGenerator.TimeLimit[pos] = val;
    }

    public void SetWarehouseX(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 24;
        val = Mathf.Clamp(val, 8, 100);
        WarehouseGenerator.BuildingSize.x = val;
        
        CalculateDistance();
    }

    public void SetWarehouseY(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 44;
        val = Mathf.Clamp(val, 8, 100);
        WarehouseGenerator.BuildingSize.y = val;
        
        CalculateDistance();
    }

    public void SetShelfX(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 2;
        val = Mathf.Clamp(val, 1, 100);
        WarehouseGenerator.ShelfColumns = val;
        
        CalculateDistance();
    }

    public void SetShelfY(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 4;
        val = Mathf.Clamp(val, 1, 100);
        WarehouseGenerator.ShelfRows = val;
        
        CalculateDistance();
    }

    private void CalculateDistance()
    {
        WarehouseGenerator.DistanceBetweenShelves = (WarehouseGenerator.BuildingSize.y - WarehouseGenerator.ShelfRows) / (WarehouseGenerator.ShelfRows + 1);
        WarehouseGenerator.DistanceToWall = (WarehouseGenerator.BuildingSize.x - WarehouseGenerator.ShelfColumns * 4.0f) / 2.0f;
        shelfSizesText.text = $"{WarehouseGenerator.DistanceBetweenShelves:0.00} m\n{WarehouseGenerator.DistanceToWall:0.00} m";
    }

    public void SetToggleBlue(Toggle input)
    {
        WarehouseGenerator.BoxTypes[0] = input.isOn;
    }
    
    public void SetToggleGreen(Toggle input)
    {
        WarehouseGenerator.BoxTypes[1] = input.isOn;
    }
    
    public void SetToggleYellow(Toggle input)
    {
        WarehouseGenerator.BoxTypes[2] = input.isOn;
    }
    
    public void SetToggleRed(Toggle input)
    {
        WarehouseGenerator.BoxTypes[3] = input.isOn;
    }

    public void SetWeightBlue(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        WarehouseGenerator.BoxWeights[0] = val;
    }
    
    public void SetWeightGreen(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        WarehouseGenerator.BoxWeights[1] = val;
    }
    
    public void SetWeightYellow(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        WarehouseGenerator.BoxWeights[2] = val;
    }
    
    public void SetWeightRed(TMP_InputField input)
    {
        if (!float.TryParse(input.text, out var val))
            val = 0.5f;
        val = Mathf.Clamp(val, 0.0f, 1.0f);
        WarehouseGenerator.BoxWeights[3] = val;
    }

    public void SetZoneMixed(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        WarehouseGenerator.ZoneSizes[0] = val;
    }
    
    public void SetZoneBlue(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        WarehouseGenerator.ZoneSizes[1] = val;
    }
    
    public void SetZoneGreen(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        WarehouseGenerator.ZoneSizes[2] = val;
    }
    
    public void SetZoneYellow(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        WarehouseGenerator.ZoneSizes[3] = val;
    }
    
    public void SetZoneRed(TMP_InputField input)
    {
        if (!int.TryParse(input.text, out var val))
            val = 1;
        val = Mathf.Clamp(val, 0, 999);
        WarehouseGenerator.ZoneSizes[4] = val;
    }
}
