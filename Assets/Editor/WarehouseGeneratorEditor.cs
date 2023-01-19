using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WarehouseGenerator))]
public class WarehouseGeneratorEditor : Editor
{
    private WarehouseGenerator _warehouseGenerator;
    private bool _foundInstance = false;

    private bool _boxTypesToggle = false;
    private bool _zonesToggle = false;
    
    public override void OnInspectorGUI()
    {
        if (!_foundInstance)
        {
            _warehouseGenerator = (WarehouseGenerator)target;
            _foundInstance = true;
        }

        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        
        _warehouseGenerator.distanceBetweenShelves = (_warehouseGenerator.buildingSize.y - _warehouseGenerator.shelfRows) / (_warehouseGenerator.shelfRows + 1);
        _warehouseGenerator.distanceToWall = (_warehouseGenerator.buildingSize.x - _warehouseGenerator.shelfColumns * 4.0f) / 2.0f;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shelf - Shelf Distance");
        EditorGUILayout.LabelField($@"{_warehouseGenerator.distanceBetweenShelves}", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shelf - Wall Distance");
        EditorGUILayout.LabelField($@"{_warehouseGenerator.distanceToWall}", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        _boxTypesToggle = EditorGUILayout.BeginFoldoutHeaderGroup(_boxTypesToggle, "Box Types");
        if (_boxTypesToggle)
        {
            for (var i = 0; i < Boxes.Consts.TypeCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _warehouseGenerator.boxTypes[i] = EditorGUILayout.Toggle(Boxes.Helpers.TypeName((Boxes.Types)Enum.ToObject(typeof(Boxes.Types), i)), _warehouseGenerator.boxTypes[i]);
                GUI.enabled = _warehouseGenerator.boxTypes[i];
                _warehouseGenerator.boxWeights[i] = EditorGUILayout.Slider("Weight: ", _warehouseGenerator.boxTypes[i] ? _warehouseGenerator.boxWeights[i] : 0, 0.0f, 1.0f);
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
            
            _warehouseGenerator.zoneSizes[0] = EditorGUILayout.IntSlider("Common zone size: ", _warehouseGenerator.zoneSizes[0], 1, _warehouseGenerator.shelfRows);

            for (var i = 0; i < Boxes.Consts.TypeCount; i++)
            {
                if (_warehouseGenerator.boxTypes[i])
                    _warehouseGenerator.zoneSizes[i + 1] = EditorGUILayout.IntSlider(Boxes.Helpers.TypeName((Boxes.Types)Enum.ToObject(typeof(Boxes.Types), i)),
                        _warehouseGenerator.zoneSizes[i + 1], 1, _warehouseGenerator.shelfRows);
                else
                    _warehouseGenerator.zoneSizes[i + 1] = 0;
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

    private string CalculateZones()
    {
        var result = "\n";
        var zoneCount = 0;
        var zoneSum = _warehouseGenerator.zoneSizes[0];
        for (var i = 0; i < Boxes.Consts.TypeCount; i++)
        {
            if (!_warehouseGenerator.boxTypes[i]) continue;
            
            zoneCount++;
            zoneSum += _warehouseGenerator.zoneSizes[i + 1];
        }

        if (zoneCount == 0)
            result += "ERROR: There are no zones specified!\n";

        if (zoneSum > _warehouseGenerator.shelfRows)
            result += "ERROR: There are more zones specified, than available rows!\n";

        if (zoneSum < _warehouseGenerator.shelfRows)
            result += "WARNING: Not all rows are used by zones! Consider increasing the mixed zone's size.\n";
        
        if (_warehouseGenerator.jobType == JobType.BoxSearching)
        {
            if (_warehouseGenerator.zoneSizes[0] < zoneCount)
                result += "WARNING: There may be more boxes to search for, than what is able to fit in the mixed zone!\n";
        }
        else
            if (_warehouseGenerator.zoneSizes[0] > zoneSum - _warehouseGenerator.zoneSizes[0])
                result += "WARNING: There may be more boxes to sort out, than what is able to fit in the zones!\n";

        if (result == "\n")
            result +=  "INFO: All looks fine!\n";

        return result;
    }
}