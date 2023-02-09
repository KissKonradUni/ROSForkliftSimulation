using System;
using System.Linq;
using Boxes;
using UnityEngine;

[SelectionBase]
public class ShelfZone : MonoBehaviour
{
    public Zones zoneType = Zones.Mixed;
    public Vector3 zoneCenter;
    public Vector3 zoneSize;
    private WarehouseGenerator _warehouseGenerator;

    private void Start()
    {
        _warehouseGenerator = WarehouseGenerator.GetInstance();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = zoneType switch
        {
            Zones.Mixed  => new Color(1.0f, 1.0f, 1.0f, 0.5f),
            Zones.Blue   => new Color(0.0f, 0.5f, 1.0f, 0.5f),
            Zones.Green  => new Color(0.3f, 1.0f, 0.3f, 0.5f),
            Zones.Yellow => new Color(1.0f, 1.0f, 0.0f, 0.5f),
            Zones.Red    => new Color(1.0f, 0.3f, 0.3f, 0.5f),
            _ => throw new ArgumentOutOfRangeException()
        };
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(zoneCenter, zoneSize);
    }

    public void AlertSame(BoxInformation box)
    {
        if (_warehouseGenerator.generating) return;
        
        if (WarehouseGenerator.JobType != JobType.BoxSorting) return;
        
        if (!WarehouseGenerator.CorrectlyPlacedBoxes[(int)box.boxType].Contains(box.gameObject))
            WarehouseGenerator.CorrectlyPlacedBoxes[(int)box.boxType].Add(box.gameObject);
    }

    public void AlertWrong(BoxInformation box)
    {
        if (_warehouseGenerator.generating) return;
        
        if (WarehouseGenerator.JobBoxes[(int)box.boxType].Count == WarehouseGenerator.CorrectlyPlacedBoxes[(int)box.boxType].Count) return;
        
        if (WarehouseGenerator.CorrectlyPlacedBoxes.Select(boxes => boxes.Count).Sum() == WarehouseGenerator.JobBoxes.Select(boxes => boxes.Count).Sum()) return;

        _warehouseGenerator.MisplacedBox();
    }
    
    public void AlertMixed(BoxInformation box)
    {
        if (_warehouseGenerator.generating) return;
        
        if (WarehouseGenerator.JobType != JobType.BoxFetching) return;

        if (WarehouseGenerator.JobBoxes[(int)box.boxType].Count >
            WarehouseGenerator.CorrectlyPlacedBoxes[(int)box.boxType].Count)
        {
            if (!WarehouseGenerator.CorrectlyPlacedBoxes[(int)box.boxType].Contains(box.gameObject))
                WarehouseGenerator.CorrectlyPlacedBoxes[(int)box.boxType].Add(box.gameObject);
        }
        else
        {
            if (WarehouseGenerator.JobBoxes[(int)box.boxType].Count == WarehouseGenerator.CorrectlyPlacedBoxes[(int)box.boxType].Count) return;
        
            if (WarehouseGenerator.CorrectlyPlacedBoxes.Select(boxes => boxes.Count).Sum() == WarehouseGenerator.JobBoxes.Select(boxes => boxes.Count).Sum()) return;
            
            _warehouseGenerator.MisplacedBox();
        }
    }
}
