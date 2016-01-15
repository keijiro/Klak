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
    [CustomEditor(typeof(ColorMapper))]
    public class ColorMapperEditor : Editor
    {
        SerializedProperty _colorMode;
        SerializedProperty _gradient;
        SerializedProperty _colorArray;
        SerializedProperty _colorEvent;

        void OnEnable()
        {
            _colorMode = serializedObject.FindProperty("_colorMode");
            _gradient = serializedObject.FindProperty("_gradient");
            _colorArray = serializedObject.FindProperty("_colorArray");
            _colorEvent = serializedObject.FindProperty("_colorEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIHelper.ShowInputValueNote();

            EditorGUILayout.PropertyField(_colorMode);

            if (_colorMode.hasMultipleDifferentValues ||
                _colorMode.enumValueIndex == (int)ColorMapper.ColorMode.Gradient)
                EditorGUILayout.PropertyField(_gradient);

            if (_colorMode.hasMultipleDifferentValues ||
                _colorMode.enumValueIndex == (int)ColorMapper.ColorMode.ColorArray)
                DrawColorArray();

            EditorGUILayout.PropertyField(_colorEvent);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawColorArray()
        {
            var len = _colorArray.arraySize;

            // FIXME: should be replaced with DelayedIntField in 5.3
            len = EditorGUILayout.IntField("Array Size", len);
            len = Mathf.Max(len, 2);

            // enlarge/shrink the list when the size is changed
            while (len > _colorArray.arraySize)
                _colorArray.InsertArrayElementAtIndex(_colorArray.arraySize - 1);
            while (len < _colorArray.arraySize)
                _colorArray.DeleteArrayElementAtIndex(_colorArray.arraySize - 1);

            EditorGUI.indentLevel++;

            for (var i = 0; i < len; i++)
            {
                var data = _colorArray.GetArrayElementAtIndex(i);
                var label = new GUIContent("Color " + (i + 1));
                EditorGUILayout.PropertyField(data, label);
            }

            EditorGUI.indentLevel--;
        }
    }
}
