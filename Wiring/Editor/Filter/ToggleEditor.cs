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
    [CustomEditor(typeof(Toggle))]
    public class ToggleEditor : Editor
    {
        SerializedProperty _offValue;
        SerializedProperty _onValue;
        SerializedProperty _interpolator;
        SerializedProperty _sendOnStartUp;
        SerializedProperty _offEvent;
        SerializedProperty _onEvent;
        SerializedProperty _valueEvent;

        static GUIContent _textStartUp = new GUIContent("Off Event on start up");

        void OnEnable()
        {
            _offValue = serializedObject.FindProperty("_offValue");
            _onValue = serializedObject.FindProperty("_onValue");
            _interpolator = serializedObject.FindProperty("_interpolator");
            _sendOnStartUp = serializedObject.FindProperty("_sendOnStartUp");
            _offEvent = serializedObject.FindProperty("_offEvent");
            _onEvent = serializedObject.FindProperty("_onEvent");
            _valueEvent = serializedObject.FindProperty("_valueEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_offValue);
            EditorGUILayout.PropertyField(_onValue);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_interpolator);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_sendOnStartUp, _textStartUp);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_offEvent);
            EditorGUILayout.PropertyField(_onEvent);
            EditorGUILayout.PropertyField(_valueEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
