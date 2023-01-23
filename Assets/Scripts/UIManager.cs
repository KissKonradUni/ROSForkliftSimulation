using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text fpsText;
    public TMP_Text timeText;
    public TMP_Text statsTitleText;
    public TMP_Text statsText;

    public WarehouseGenerator warehouseGenerator;
    public ForkliftMain forklift;
    
    private float _start = -1.0f;

    private int _boxCount;
    
    private void Start()
    {
        forklift = ForkliftMain.GetInstance();
        
        statsTitleText.text = 
            "Time limit:\n" +
            "Job type:\n" +
            $"{(warehouseGenerator.jobType == JobType.BoxSearching ? "Fetch:" : "Sort:")}\n" +
            "Collisions:\n" +
            "Box count:\n" +
            "Misplaced:\n" +
            "Trip length:";
        
        foreach (var jobBoxes in warehouseGenerator.JobBoxes)
            _boxCount += jobBoxes.Count;
    }

    private void Update()
    {
        if (Math.Abs(_start - (-1.0f)) < 0.01f)
            _start = Time.time;

        var elapsed = Time.time - _start;
        
        fpsText.text = $"FPS: {(1 / Time.deltaTime):0}";

        timeText.text = $"Time: {Mathf.FloorToInt(elapsed / 3600f):00}:{Mathf.FloorToInt(elapsed / 60f):00}:{Mathf.FloorToInt(elapsed % 60f):00}";

        statsText.text =
            $"{warehouseGenerator.timeLimit.x:00}:{warehouseGenerator.timeLimit.y:00}:{warehouseGenerator.timeLimit.z:00}\n" +
            $"{(warehouseGenerator.jobType == JobType.BoxSearching ? "Fetch" : "Sort")}\n" +
            $"{warehouseGenerator.JobBoxes[0].Count}<color=#88f>B<color=white> {warehouseGenerator.JobBoxes[1].Count}<color=#8f8>G<color=white> {warehouseGenerator.JobBoxes[2].Count}<color=#ff4>Y<color=white> {warehouseGenerator.JobBoxes[3].Count}<color=#f88>R<color=white>\n" +
             "0\n" +
            $"{warehouseGenerator.correctlyPlacedBoxes.Sum()} / {_boxCount}\n" +
            $"{warehouseGenerator.misplacedBoxes}\n" +
            $"{forklift.distanceTraveled:0.00} m";
    }
}
