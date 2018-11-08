// Klak - Creative coding library for Unity
// https://github.com/keijiro/Klak

using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BangFilter))]
    public class BangFilterEditor : Editor
    {
        SerializedProperty _state;
        SerializedProperty _bangEvent;

        static GUIContent _textInitialState = new GUIContent("Opened");

        void OnEnable()
        {
            _state = serializedObject.FindProperty("_state");
            _bangEvent = serializedObject.FindProperty("_bangEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_state, _textInitialState);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_bangEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
