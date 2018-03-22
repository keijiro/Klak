// Klak - Creative coding library for Unity
// https://github.com/keijiro/Klak

using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RandomValue))]
    public class RandomValueEditor : Editor
    {
        SerializedProperty _minimum;
        SerializedProperty _maximum;
        SerializedProperty _sendOnStartUp;
        SerializedProperty _outputEvent;

        void OnEnable()
        {
            _minimum = serializedObject.FindProperty("_minimum");
            _maximum = serializedObject.FindProperty("_maximum");
            _sendOnStartUp = serializedObject.FindProperty("_sendOnStartUp");
            _outputEvent = serializedObject.FindProperty("_outputEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_minimum);
            EditorGUILayout.PropertyField(_maximum);
            EditorGUILayout.PropertyField(_sendOnStartUp);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_outputEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
