using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemplateManager : MonoBehaviour
{
    public TMP_InputField   seedIf;
    public TMP_Dropdown     jobTypeDd;
    public TMP_InputField   completionRequirementIf;
    public TMP_InputField[] timeLimitIf = new TMP_InputField[3];
    public TMP_InputField[] warehouseSizeIf = new TMP_InputField[2];
    public TMP_InputField[] shelfCountIf = new TMP_InputField[2];
    public TMP_Text         shelfDistanceTx;
    public Toggle[]         boxTypeTg = new Toggle[4];
    public TMP_InputField[] boxWeightIf = new TMP_InputField[4];
    public TMP_InputField[] zoneSizeIf = new TMP_InputField[5];

    private void Start()
    {
        seedIf.text = WarehouseGenerator.Seed.ToString();
        jobTypeDd.value = (int)WarehouseGenerator.JobType;
        completionRequirementIf.text = WarehouseGenerator.SearchPercent.ToString();
        timeLimitIf[0].text = $"{WarehouseGenerator.TimeLimit.x:00}";
        timeLimitIf[1].text = $"{WarehouseGenerator.TimeLimit.y:00}";
        timeLimitIf[2].text = $"{WarehouseGenerator.TimeLimit.z:00}";
        warehouseSizeIf[0].text = $"{WarehouseGenerator.BuildingSize.x:0}";
        warehouseSizeIf[1].text = $"{WarehouseGenerator.BuildingSize.y:0}";
        shelfCountIf[0].text = $"{WarehouseGenerator.ShelfColumns}";
        shelfCountIf[1].text = $"{WarehouseGenerator.ShelfRows}";
        shelfDistanceTx.text = $"{WarehouseGenerator.DistanceBetweenShelves:0.00} m\n{WarehouseGenerator.DistanceToWall:0.00} m\n";
        boxTypeTg[0].isOn = WarehouseGenerator.BoxTypes[0];
        boxTypeTg[1].isOn = WarehouseGenerator.BoxTypes[1];
        boxTypeTg[2].isOn = WarehouseGenerator.BoxTypes[2];
        boxTypeTg[3].isOn = WarehouseGenerator.BoxTypes[3];
        boxWeightIf[0].text = $"{WarehouseGenerator.BoxWeights[0]:0.00}";
        boxWeightIf[1].text = $"{WarehouseGenerator.BoxWeights[1]:0.00}";
        boxWeightIf[2].text = $"{WarehouseGenerator.BoxWeights[2]:0.00}";
        boxWeightIf[3].text = $"{WarehouseGenerator.BoxWeights[3]:0.00}";
        zoneSizeIf[0].text = $"{WarehouseGenerator.ZoneSizes[0]:0}";
        zoneSizeIf[1].text = $"{WarehouseGenerator.ZoneSizes[1]:0}";
        zoneSizeIf[2].text = $"{WarehouseGenerator.ZoneSizes[2]:0}";
        zoneSizeIf[3].text = $"{WarehouseGenerator.ZoneSizes[3]:0}";
        zoneSizeIf[4].text = $"{WarehouseGenerator.ZoneSizes[4]:0}";
    }
}