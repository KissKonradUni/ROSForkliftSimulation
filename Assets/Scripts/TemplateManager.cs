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
        SetTextFields();
    }

    private void SetTextFields()
    {
        seedIf.text                  = WarehouseGenerator.Seed.ToString();
        jobTypeDd.value              = (int)WarehouseGenerator.JobType;
        completionRequirementIf.text = WarehouseGenerator.SearchPercent.ToString();
        timeLimitIf[0].text          = $"{WarehouseGenerator.TimeLimit.x:00}";
        timeLimitIf[1].text          = $"{WarehouseGenerator.TimeLimit.y:00}";
        timeLimitIf[2].text          = $"{WarehouseGenerator.TimeLimit.z:00}";
        warehouseSizeIf[0].text      = $"{WarehouseGenerator.BuildingSize.x:0}";
        warehouseSizeIf[1].text      = $"{WarehouseGenerator.BuildingSize.y:0}";
        shelfCountIf[0].text         = $"{WarehouseGenerator.ShelfColumns}";
        shelfCountIf[1].text         = $"{WarehouseGenerator.ShelfRows}";
        shelfDistanceTx.text         = $"{WarehouseGenerator.DistanceBetweenShelves:0.00} m\n{WarehouseGenerator.DistanceToWall:0.00} m\n";
        boxTypeTg[0].isOn            = WarehouseGenerator.BoxTypes[0];
        boxTypeTg[1].isOn            = WarehouseGenerator.BoxTypes[1];
        boxTypeTg[2].isOn            = WarehouseGenerator.BoxTypes[2];
        boxTypeTg[3].isOn            = WarehouseGenerator.BoxTypes[3];
        boxWeightIf[0].text          = $"{WarehouseGenerator.BoxWeights[0]:0.00}";
        boxWeightIf[1].text          = $"{WarehouseGenerator.BoxWeights[1]:0.00}";
        boxWeightIf[2].text          = $"{WarehouseGenerator.BoxWeights[2]:0.00}";
        boxWeightIf[3].text          = $"{WarehouseGenerator.BoxWeights[3]:0.00}";
        zoneSizeIf[0].text           = $"{WarehouseGenerator.ZoneSizes[0]:0}";
        zoneSizeIf[1].text           = $"{WarehouseGenerator.ZoneSizes[1]:0}";
        zoneSizeIf[2].text           = $"{WarehouseGenerator.ZoneSizes[2]:0}";
        zoneSizeIf[3].text           = $"{WarehouseGenerator.ZoneSizes[3]:0}";
        zoneSizeIf[4].text           = $"{WarehouseGenerator.ZoneSizes[4]:0}";
    }

    public void SetTemplate(TMP_Dropdown input)
    {
        switch (input.value)
        {
            case 0:
                WarehouseGenerator.Seed = -1;
                WarehouseGenerator.JobType = JobType.BoxFetching;
                WarehouseGenerator.SearchPercent = 50;
                WarehouseGenerator.TimeLimit = new Vector3(0.0f, 15.0f, 0.0f);
                WarehouseGenerator.BuildingSize = new Vector2(24.0f, 44.0f);
                WarehouseGenerator.ShelfColumns = 2;
                WarehouseGenerator.ShelfRows = 4;
                WarehouseGenerator.BoxTypes = new[] { true, true, false, false };
                WarehouseGenerator.BoxWeights = new[] { 0.5f, 0.5f, 0.0f, 0.0f };
                WarehouseGenerator.ZoneSizes = new[] { 2, 1, 1, 0, 0 };
                break;
            case 1:
                WarehouseGenerator.Seed = -1;
                WarehouseGenerator.JobType = JobType.BoxSorting;
                WarehouseGenerator.SearchPercent = 100;
                WarehouseGenerator.TimeLimit = new Vector3(0.0f, 15.0f, 0.0f);
                WarehouseGenerator.BuildingSize = new Vector2(24.0f, 44.0f);
                WarehouseGenerator.ShelfColumns = 2;
                WarehouseGenerator.ShelfRows = 4;
                WarehouseGenerator.BoxTypes = new[] { true, true, false, false };
                WarehouseGenerator.BoxWeights = new[] { 0.25f, 0.25f, 0.0f, 0.0f };
                WarehouseGenerator.ZoneSizes = new[] { 2, 1, 1, 0, 0 };
                break;
            case 2:
                WarehouseGenerator.Seed = -1;
                WarehouseGenerator.JobType = JobType.BoxFetching;
                WarehouseGenerator.SearchPercent = 50;
                WarehouseGenerator.TimeLimit = new Vector3(0.0f, 30.0f, 0.0f);
                WarehouseGenerator.BuildingSize = new Vector2(24.0f, 60.0f);
                WarehouseGenerator.ShelfColumns = 2;
                WarehouseGenerator.ShelfRows = 6;
                WarehouseGenerator.BoxTypes = new[] { true, true, true, true };
                WarehouseGenerator.BoxWeights = new[] { 0.5f, 0.5f, 0.5f, 0.5f };
                WarehouseGenerator.ZoneSizes = new[] { 2, 1, 1, 1, 1 };
                break;
            case 3:
                WarehouseGenerator.Seed = -1;
                WarehouseGenerator.JobType = JobType.BoxSorting;
                WarehouseGenerator.SearchPercent = 100;
                WarehouseGenerator.TimeLimit = new Vector3(0.0f, 30.0f, 0.0f);
                WarehouseGenerator.BuildingSize = new Vector2(24.0f, 60.0f);
                WarehouseGenerator.ShelfColumns = 2;
                WarehouseGenerator.ShelfRows = 6;
                WarehouseGenerator.BoxTypes = new[] { true, true, true, true };
                WarehouseGenerator.BoxWeights = new[] { 0.25f, 0.25f, 0.25f, 0.25f };
                WarehouseGenerator.ZoneSizes = new[] { 2, 1, 1, 1, 1 };
                break;
            case 4:
                WarehouseGenerator.Seed = -1;
                WarehouseGenerator.JobType = JobType.BoxFetching;
                WarehouseGenerator.SearchPercent = 50;
                WarehouseGenerator.TimeLimit = new Vector3(0.0f, 45.0f, 0.0f);
                WarehouseGenerator.BuildingSize = new Vector2(32.0f, 60.0f);
                WarehouseGenerator.ShelfColumns = 4;
                WarehouseGenerator.ShelfRows = 6;
                WarehouseGenerator.BoxTypes = new[] { true, true, true, true };
                WarehouseGenerator.BoxWeights = new[] { 0.5f, 0.5f, 0.5f, 0.5f };
                WarehouseGenerator.ZoneSizes = new[] { 2, 1, 1, 1, 1 };
                break;
            case 5:
                WarehouseGenerator.Seed = -1;
                WarehouseGenerator.JobType = JobType.BoxSorting;
                WarehouseGenerator.SearchPercent = 100;
                WarehouseGenerator.TimeLimit = new Vector3(0.0f, 45.0f, 0.0f);
                WarehouseGenerator.BuildingSize = new Vector2(32.0f, 60.0f);
                WarehouseGenerator.ShelfColumns = 4;
                WarehouseGenerator.ShelfRows = 6;
                WarehouseGenerator.BoxTypes = new[] { true, true, true, true };
                WarehouseGenerator.BoxWeights = new[] { 0.25f, 0.25f, 0.25f, 0.25f };
                WarehouseGenerator.ZoneSizes = new[] { 2, 1, 1, 1, 1 };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(input));
        }
        SetTextFields();
    }
}