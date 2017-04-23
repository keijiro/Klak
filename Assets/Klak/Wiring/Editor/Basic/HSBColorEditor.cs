// Klak - Creative coding library for Unity
// https://github.com/keijiro/Klak

using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HSBColor))]
    public class HSBColorEditor : Editor
    {
        SerializedProperty _hue;
        SerializedProperty _saturation;
        SerializedProperty _brightness;
        SerializedProperty _alpha;
        SerializedProperty _colorEvent;

        static GUIContent _textHue = new GUIContent("Initial Hue");
        static GUIContent _textSaturation = new GUIContent("Initial Saturation");
        static GUIContent _textBrightness = new GUIContent("Initial Brightness");
        static GUIContent _textAlpha = new GUIContent("Initial Alpha");

        void OnEnable()
        {
            _hue = serializedObject.FindProperty("_hue");
            _saturation = serializedObject.FindProperty("_saturation");
            _brightness = serializedObject.FindProperty("_brightness");
            _alpha = serializedObject.FindProperty("_alpha");
            _colorEvent = serializedObject.FindProperty("_colorEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_hue, _textHue);
            EditorGUILayout.PropertyField(_saturation, _textSaturation);
            EditorGUILayout.PropertyField(_brightness, _textBrightness);
            EditorGUILayout.PropertyField(_alpha, _textAlpha);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_colorEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
