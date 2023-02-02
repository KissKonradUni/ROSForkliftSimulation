using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WarehouseGenerator))]
public class WarehouseGeneratorEditor : Editor
{
    private WarehouseGenerator _warehouseGenerator;
    private bool _foundInstance;

    private bool _boxTypesToggle;
    private bool _zonesToggle;
    
    public override void OnInspectorGUI()
    {
        if (!_foundInstance)
        {
            _warehouseGenerator = (WarehouseGenerator)target;
            _foundInstance = true;
        }

        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        
        WarehouseGenerator.DistanceBetweenShelves = (WarehouseGenerator.BuildingSize.y - WarehouseGenerator.ShelfRows) / (WarehouseGenerator.ShelfRows + 1);
        WarehouseGenerator.DistanceToWall = (WarehouseGenerator.BuildingSize.x - WarehouseGenerator.ShelfColumns * 4.0f) / 2.0f;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shelf - Shelf Distance");
        EditorGUILayout.LabelField($@"{WarehouseGenerator.DistanceBetweenShelves}", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shelf - Wall Distance");
        EditorGUILayout.LabelField($@"{WarehouseGenerator.DistanceToWall}", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        _boxTypesToggle = EditorGUILayout.BeginFoldoutHeaderGroup(_boxTypesToggle, "Box Types");
        if (_boxTypesToggle)
        {
            for (var i = 0; i < Boxes.Consts.TypeCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                WarehouseGenerator.BoxTypes[i] = EditorGUILayout.Toggle(Boxes.Helpers.TypeName((Boxes.Types)Enum.ToObject(typeof(Boxes.Types), i)), WarehouseGenerator.BoxTypes[i]);
                GUI.enabled = WarehouseGenerator.BoxTypes[i];
                WarehouseGenerator.BoxWeights[i] = EditorGUILayout.Slider("Weight: ", WarehouseGenerator.BoxTypes[i] ? WarehouseGenerator.BoxWeights[i] : 0, 0.0f, 1.0f);
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        _zonesToggle = EditorGUILayout.BeginFoldoutHeaderGroup(_zonesToggle, "Zones");
        if (_zonesToggle)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.enabled = false;
            EditorGUILayout.TextArea(CalculateZones(), EditorStyles.boldLabel);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            
            WarehouseGenerator.ZoneSizes[0] = EditorGUILayout.IntSlider("Common zone size: ", WarehouseGenerator.ZoneSizes[0], 1, WarehouseGenerator.ShelfRows);

            for (var i = 0; i < Boxes.Consts.TypeCount; i++)
            {
                if (WarehouseGenerator.BoxTypes[i])
                    WarehouseGenerator.ZoneSizes[i + 1] = EditorGUILayout.IntSlider(Boxes.Helpers.TypeName((Boxes.Types)Enum.ToObject(typeof(Boxes.Types), i)),
                        WarehouseGenerator.ZoneSizes[i + 1], 1, WarehouseGenerator.ShelfRows);
                else
                    WarehouseGenerator.ZoneSizes[i + 1] = 0;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space(10.0f);

        if (GUILayout.Button("Generate"))
        {
            _warehouseGenerator.GenerateRoom();
        }
    }

    public static string CalculateZones()
    {
        var result = "\n";
        var zoneCount = 0;
        var zoneSum = WarehouseGenerator.ZoneSizes[0];
        for (var i = 0; i < Boxes.Consts.TypeCount; i++)
        {
            if (!WarehouseGenerator.BoxTypes[i]) continue;
            
            zoneCount++;
            zoneSum += WarehouseGenerator.ZoneSizes[i + 1];
        }

        if (zoneCount == 0)
            result += "ERROR: There are no zones specified!\n";

        if (zoneSum > WarehouseGenerator.ShelfRows)
            result += "ERROR: There are more zones specified, than available rows!\n";

        if (zoneSum < WarehouseGenerator.ShelfRows)
            result += "WARNING: Not all rows are used by zones! Consider increasing the mixed zone's size.\n";
        
        if (WarehouseGenerator.JobType == JobType.BoxSearching)
        {
            if (WarehouseGenerator.ZoneSizes[0] < zoneCount)
                result += "WARNING: There may be more boxes to search for, than what is able to fit in the mixed zone!\n";
        }
        else
            if (WarehouseGenerator.ZoneSizes[0] > zoneSum - WarehouseGenerator.ZoneSizes[0])
                result += "WARNING: There may be more boxes to sort out, than what is able to fit in the zones!\n";

        if (result == "\n")
            result +=  "INFO: All looks fine!\n";

        return result;
    }
}