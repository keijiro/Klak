//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TransformMap))]
    public class TransformMapEditor : Editor
    {
        SerializedProperty _translationMode;
        SerializedProperty _translationVector;
        SerializedProperty _translationAmount0;
        SerializedProperty _translationAmount1;

        SerializedProperty _rotationMode;
        SerializedProperty _rotationAxis;
        SerializedProperty _rotationAngle0;
        SerializedProperty _rotationAngle1;

        SerializedProperty _scaleMode;
        SerializedProperty _scaleVector;
        SerializedProperty _scaleAmount0;
        SerializedProperty _scaleAmount1;

        SerializedProperty _targetTransform;
        SerializedProperty _addToOriginal;

        static GUIContent _textAmount0 = new GUIContent("Amount at 0");
        static GUIContent _textAmount1 = new GUIContent("Amount at 1");
        static GUIContent _textAngle0 = new GUIContent("Angle at 0");
        static GUIContent _textAngle1 = new GUIContent("Angle at 1");
        static GUIContent _textRotation = new GUIContent("Rotation");
        static GUIContent _textScale = new GUIContent("Scale");
        static GUIContent _textTranslation = new GUIContent("Translation");

        void OnEnable()
        {
            _translationMode = serializedObject.FindProperty("_translationMode");
            _translationVector = serializedObject.FindProperty("_translationVector");
            _translationAmount0 = serializedObject.FindProperty("_translationAmount0");
            _translationAmount1 = serializedObject.FindProperty("_translationAmount1");

            _rotationMode = serializedObject.FindProperty("_rotationMode");
            _rotationAxis = serializedObject.FindProperty("_rotationAxis");
            _rotationAngle0 = serializedObject.FindProperty("_rotationAngle0");
            _rotationAngle1 = serializedObject.FindProperty("_rotationAngle1");

            _scaleMode = serializedObject.FindProperty("_scaleMode");
            _scaleVector = serializedObject.FindProperty("_scaleVector");
            _scaleAmount0 = serializedObject.FindProperty("_scaleAmount0");
            _scaleAmount1 = serializedObject.FindProperty("_scaleAmount1");

            _targetTransform = serializedObject.FindProperty("_targetTransform");
            _addToOriginal = serializedObject.FindProperty("_addToOriginal");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIHelper.ShowInputValueNote();

            EditorGUILayout.PropertyField(_targetTransform);

            // translation

            EditorGUILayout.PropertyField(_translationMode, _textTranslation);

            EditorGUI.indentLevel++;

            var showAll = _translationMode.hasMultipleDifferentValues;
            var t_mode = (TransformMap.TranslationMode)_translationMode.enumValueIndex;

            if (showAll || t_mode == TransformMap.TranslationMode.Vector)
                EditorGUILayout.PropertyField(_translationVector, GUIContent.none);

            if (showAll || t_mode != TransformMap.TranslationMode.Off)
            {
                EditorGUILayout.PropertyField(_translationAmount0, _textAmount0);
                EditorGUILayout.PropertyField(_translationAmount1, _textAmount1);
            }

            EditorGUI.indentLevel--;

            // rotation

            EditorGUILayout.PropertyField(_rotationMode, _textRotation);

            EditorGUI.indentLevel++;

            showAll = _rotationMode.hasMultipleDifferentValues;
            var r_mode = (TransformMap.RotationMode)_rotationMode.enumValueIndex;

            if (showAll || r_mode == TransformMap.RotationMode.Vector)
                EditorGUILayout.PropertyField(_rotationAxis, GUIContent.none);

            if (showAll || r_mode != TransformMap.RotationMode.Off)
            {
                EditorGUILayout.PropertyField(_rotationAngle0, _textAngle0);
                EditorGUILayout.PropertyField(_rotationAngle1, _textAngle1);
            }

            EditorGUI.indentLevel--;

            // scale

            EditorGUILayout.PropertyField(_scaleMode, _textScale);

            EditorGUI.indentLevel++;

            showAll = _rotationMode.hasMultipleDifferentValues;
            var s_mode = (TransformMap.ScaleMode)_scaleMode.enumValueIndex;

            if (showAll || s_mode == TransformMap.ScaleMode.Vector)
                EditorGUILayout.PropertyField(_scaleVector, GUIContent.none);

            if (showAll || s_mode != TransformMap.ScaleMode.Off)
            {
                EditorGUILayout.PropertyField(_scaleAmount0, _textAmount0);
                EditorGUILayout.PropertyField(_scaleAmount1, _textAmount1);
            }

            EditorGUI.indentLevel--;

            // etc.

            EditorGUILayout.PropertyField(_addToOriginal);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
