using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RosTestServer))]
public class RosTestServerEditor : Editor
{
    private RosTestServer _targetComponent;
    private int _popupPosition;
    private float _sendValueFloat32;
    private Vector3 _sendValueVec3;
    private Vector3 _sendValueRotation;
    private bool _buttonLastState;
    private bool _active = true, _lastActive = true;
    private bool _manual = true, _lastManual = true;
    
    public override void OnInspectorGUI()
    {
        _targetComponent = (RosTestServer)target;
        _active = _targetComponent.isActiveAndEnabled;
        
        EditorGUILayout.LabelField("Server: ", EditorStyles.boldLabel);
        GUI.enabled = !Application.isPlaying;
        _active = EditorGUILayout.Toggle("Active: ", _active);
        if (_active != _lastActive)
        {
            _targetComponent.gameObject.SetActive(_active);
            _lastActive = _active;
        }
        GUI.enabled = true;
        _manual = EditorGUILayout.Toggle("Manual: ", _manual);
        if (_manual != _lastManual)
        {
            _targetComponent.manualDriving = _manual;
            _lastManual = _manual;
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Listeners: ", EditorStyles.boldLabel);
        
        foreach (var listener in _targetComponent.Listeners)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(listener.Key);

            if (listener.Key.Contains("sensor") && listener.Value.StartsWith("C"))
            {
                var split = listener.Value.Split(" ");
                var color = new Color(float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
                //GUI.enabled = false;
                EditorGUILayout.ColorField(color);
                GUI.enabled = true;
            } else
                EditorGUILayout.LabelField(listener.Value);
            
            EditorGUILayout.EndHorizontal();   
        }
        
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Publishers: ", EditorStyles.boldLabel);
        
        foreach (var publisher in _targetComponent.Publishers)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(publisher.Key);
            EditorGUILayout.LabelField(publisher.Value.ToString());
            EditorGUILayout.EndHorizontal();   
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Publish message: ", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _popupPosition = EditorGUILayout.Popup(_popupPosition, _targetComponent.Publishers.Keys.ToArray());
        _sendValueFloat32 = EditorGUILayout.FloatField(_sendValueFloat32);
        EditorGUILayout.EndHorizontal();
        GUI.enabled = Application.isPlaying;
        var buttonState = GUILayout.Button("Send");
        if (_buttonLastState != buttonState)
        {
            if (buttonState)
            {
               RosManager.GetInstance().TestReceiveMessage(_targetComponent.Publishers.Keys.ToArray()[_popupPosition], _sendValueFloat32); 
            }

            _buttonLastState = buttonState;
        }
        GUI.enabled = true;

        EditorUtility.SetDirty( target );
    }
}