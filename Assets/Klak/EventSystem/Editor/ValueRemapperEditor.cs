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

namespace Klak
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ValueRemapper))]
    public class ValueRemapperEditor : Editor
    {
        SerializedProperty _inputCurve;
        SerializedProperty _modulationType;
        SerializedProperty _modulationCurve;
        SerializedProperty _outputType;

        SerializedProperty _threshold;

        SerializedProperty _intOutput0;
        SerializedProperty _intOutput1;

        SerializedProperty _floatOutput0;
        SerializedProperty _floatOutput1;

        SerializedProperty _vector3Output0;
        SerializedProperty _vector3Output1;

        SerializedProperty _boolEvent;
        SerializedProperty _intEvent;
        SerializedProperty _floatEvent;
        SerializedProperty _vector3Event;

        static GUIContent _textCurve = new GUIContent("Curve");
        static GUIContent _textModulation = new GUIContent("Modulation");
        static GUIContent _textOutput0 = new GUIContent("Value at 0");
        static GUIContent _textOutput1 = new GUIContent("Value at 1");

        void OnEnable()
        {
            _inputCurve = serializedObject.FindProperty("_inputCurve");
            _modulationType = serializedObject.FindProperty("_modulationType");
            _modulationCurve = serializedObject.FindProperty("_modulationCurve");
            _outputType = serializedObject.FindProperty("_outputType");

            _threshold = serializedObject.FindProperty("_threshold");

            _intOutput0 = serializedObject.FindProperty("_intOutput0");
            _intOutput1 = serializedObject.FindProperty("_intOutput1");

            _floatOutput0 = serializedObject.FindProperty("_floatOutput0");
            _floatOutput1 = serializedObject.FindProperty("_floatOutput1");

            _vector3Output0 = serializedObject.FindProperty("_vector3Output0");
            _vector3Output1 = serializedObject.FindProperty("_vector3Output1");

            _boolEvent = serializedObject.FindProperty("_boolEvent");
            _intEvent = serializedObject.FindProperty("_intEvent");
            _floatEvent = serializedObject.FindProperty("_floatEvent");
            _vector3Event = serializedObject.FindProperty("_vector3Event");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIHelper.ShowInputValueNote();

            EditorGUILayout.PropertyField(_inputCurve);
            EditorGUILayout.PropertyField(_modulationType, _textModulation);

            if (_modulationType.hasMultipleDifferentValues ||
                _modulationType.enumValueIndex != 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_modulationCurve, _textCurve);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_outputType);

            var showAllTypes = _outputType.hasMultipleDifferentValues;
            var type = (ValueRemapper.OutputType)_outputType.enumValueIndex;

            if (showAllTypes || type == ValueRemapper.OutputType.Bool)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_threshold);
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(_boolEvent);
            }

            if (showAllTypes || type == ValueRemapper.OutputType.Int)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_intOutput0, _textOutput0);
                EditorGUILayout.PropertyField(_intOutput1, _textOutput1);
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(_intEvent);
            }

            if (showAllTypes || type == ValueRemapper.OutputType.Float)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_floatOutput0, _textOutput0);
                EditorGUILayout.PropertyField(_floatOutput1, _textOutput1);
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(_floatEvent);
            }

            if (showAllTypes || type == ValueRemapper.OutputType.Vector3)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_vector3Output0, _textOutput0);
                EditorGUILayout.PropertyField(_vector3Output1, _textOutput1);
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(_vector3Event);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
