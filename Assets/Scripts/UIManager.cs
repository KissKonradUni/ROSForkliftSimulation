using System;
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

    private void Start()
    {
        forklift = ForkliftMain.GetInstance();
        
        statsTitleText.text = 
            "Job type:\n" +
            $"{(warehouseGenerator.jobType == JobType.BoxSearching ? "Fetch:" : "Sort:")}\n" +
            "Collisions:\n" +
            "Box count:\n" +
            "Misplaced:\n" +
            "Trip length:";
    }

    private void Update()
    {
        if (Math.Abs(_start - (-1.0f)) < 0.01f)
            _start = Time.time;

        var elapsed = Time.time - _start;
        
        fpsText.text = $"FPS: {(1 / Time.deltaTime):0}";

        timeText.text = $"Time: {Mathf.FloorToInt(elapsed / 3600f):00}:{Mathf.FloorToInt(elapsed / 60f):00}:{Mathf.FloorToInt(elapsed % 60f):00}";

        statsText.text =
            $"{(warehouseGenerator.jobType == JobType.BoxSearching ? "Fetch" : "Sort")}\n" +
             "0<color=#88f>B<color=white> 0<color=#8f8>G<color=white> 0<color=#ff4>Y<color=white> 0<color=#f88>R<color=white>\n" +
            $"0\n" +
            $"0 / NaN\n" +
            $"0\n" +
            $"{forklift.DistanceTravelled:.00} m";
    }
}
