using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ConstantInput))]
    public class ConstantInputEditor:Editor
    {
        SerializedProperty _value;
        SerializedProperty _outputEvent;

        void OnEnable()
        {
            _value=serializedObject.FindProperty("_value");
            _outputEvent = serializedObject.FindProperty("_outputEvent");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_value);
            EditorGUILayout.PropertyField(_outputEvent);
            serializedObject.ApplyModifiedProperties();           
        }
    }
}
