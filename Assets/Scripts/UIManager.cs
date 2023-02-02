using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text fpsText;
    public TMP_Text timeText;
    public TMP_Text statsTitleText;
    public TMP_Text statsText;

    [Header("Game end menu")] 
    public GameObject gameEndMenu;
    public TMP_Text statNumbersText;
    public TMP_Text statScoresText;
    
    [Header("Singletons")]
    public ForkliftMain forklift;

    private float _start = -1.0f;
    private float _timeLimit;

    private int _boxCount;

    private bool _started;

    private void Start()
    {
        StartCoroutine(LateStart());
        
        gameEndMenu.SetActive(false);
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1.0f);
     
        forklift = ForkliftMain.GetInstance();

        statsTitleText.text = 
            "Time limit:\n" +
            "Job type:\n" +
            $"{(WarehouseGenerator.JobType == JobType.BoxSearching ? "Fetch:" : "Sort:")}\n" +
            "Box count:\n" +
            "Misplaced:\n" +
            "Trip length:";
        
        foreach (var jobBoxes in WarehouseGenerator.JobBoxes)
            _boxCount += jobBoxes.Count;

        if (_boxCount == 0)
            _boxCount = 1;

        _started = true;

        _timeLimit = WarehouseGenerator.TimeLimit.x * 3600 +
                     WarehouseGenerator.TimeLimit.y * 60 +
                     WarehouseGenerator.TimeLimit.z;
    }
    
    private void Update()
    {
        if (!_started)
            return;

        if (Math.Abs(_start - (-1.0f)) < 0.01f)
            _start = Time.time;
        
        var elapsed = Time.time - _start;

        var correctlyPlacedCount = WarehouseGenerator.CorrectlyPlacedBoxes.Select(boxes => boxes.Count).Sum();
        
        fpsText.text = $"FPS: {(1 / Time.deltaTime):0}";

        timeText.text = $"Time: {Mathf.FloorToInt(elapsed / 3600f):00}:{Mathf.FloorToInt(elapsed / 60f):00}:{Mathf.FloorToInt(elapsed % 60f):00}";

        statsText.text =
            $"{WarehouseGenerator.TimeLimit.x:00}:{WarehouseGenerator.TimeLimit.y:00}:{WarehouseGenerator.TimeLimit.z:00}\n" +
            $"{(WarehouseGenerator.JobType == JobType.BoxSearching ? "Fetch" : "Sort")}\n" +
            $"{WarehouseGenerator.JobBoxes[0].Count}<color=#88f>B<color=white> {WarehouseGenerator.JobBoxes[1].Count}<color=#8f8>G<color=white> {WarehouseGenerator.JobBoxes[2].Count}<color=#ff4>Y<color=white> {WarehouseGenerator.JobBoxes[3].Count}<color=#f88>R<color=white>\n" +
            $"{correctlyPlacedCount} / {_boxCount}\n" +
            $"{WarehouseGenerator.MisplacedBoxes}\n" +
            $"{forklift.distanceTraveled:0.00} m";

        if (elapsed < _timeLimit && correctlyPlacedCount < _boxCount) return;
        
        RosManager.GetInstance().Disconnect();

        var boxScore = correctlyPlacedCount * 100;
        var timeScore = (1.0f - elapsed / _timeLimit) * (_timeLimit / (15 * 60) * 1000) * (correctlyPlacedCount / (_boxCount * 1.0f));
        var incorrectScore = WarehouseGenerator.MisplacedBoxes * -100;
        var tripScore = (1.0f - forklift.distanceTraveled / (_boxCount * 100.0f)) * 10.0f;
        var difficultyModifier = 1.0f + (_boxCount / 10.0f) + Mathf.Min(1.0f - _timeLimit / (15 * 60), 0.0f);
        var score = boxScore + timeScore + incorrectScore + tripScore;
        
        statScoresText.text =
             "" +
            $"\n+ {timeScore:0}" +
             "\n" + 
            $"\n+ {boxScore:0}" + 
            $"\n<color=\"{(incorrectScore < 0 ? "red" : "white")}\">- {incorrectScore:0}</color>" + 
            $"\n<color=\"{(tripScore < 0 ? "red" : "white")}\">{(tripScore < 0 ? "-" : "+")} {tripScore:0}</color>" +
            $"\n+ {(score * difficultyModifier - score):0}";

        score *= difficultyModifier;
        
        statNumbersText.text =
            $"{WarehouseGenerator.TimeLimit.x:00}:{WarehouseGenerator.TimeLimit.y:00}:{WarehouseGenerator.TimeLimit.z:00}" +
            $"\n{Mathf.FloorToInt(elapsed / 3600f):00}:{Mathf.FloorToInt(elapsed / 60f):00}:{Mathf.FloorToInt(elapsed % 60f):00}" +
            $"\n{_boxCount}" +
            $"\n{correctlyPlacedCount}" +
            $"\n{WarehouseGenerator.MisplacedBoxes}" +
            $"\n{forklift.distanceTraveled:0.00} m" +
            $"\n{difficultyModifier}x" +
             "\n" +
            $"\n{StaticGameManager.Username}" +
            $"\n{score:0}";

        _started = false;
        
        HighscoreManager.Load();
        HighscoreManager.Highscores.Add(new Score(StaticGameManager.Username, Mathf.RoundToInt(score)));
        HighscoreManager.Save();
        
        gameEndMenu.SetActive(true);
    }

    public void BackToMainMenu()
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
}
