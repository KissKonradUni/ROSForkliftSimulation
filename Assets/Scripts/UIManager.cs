using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text fpsText;
    public TMP_Text timeText;

    private float _start = -1.0f;

    private void Update()
    {
        if (Math.Abs(_start - (-1.0f)) < 0.01f)
            _start = Time.time;

        var elapsed = Time.time - _start;
        
        fpsText.text = $"FPS: {(1 / Time.deltaTime):0}";

        timeText.text = $"Time: {Mathf.FloorToInt(elapsed / 3600f):00}:{Mathf.FloorToInt(elapsed / 60f):00}:{Mathf.FloorToInt(elapsed % 60f):00}";
    }
}
