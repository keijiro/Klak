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
    [CustomEditor(typeof(ToggleFour))]
    public class ToggleFourEditor : Editor
    {
        SerializedProperty _stateCount;
        SerializedProperty _value1;
        SerializedProperty _value2;
        SerializedProperty _value3;
        SerializedProperty _value4;
        SerializedProperty _interpolator;
        SerializedProperty _sendOnStartUp;
        SerializedProperty _state1Event;
        SerializedProperty _state2Event;
        SerializedProperty _state3Event;
        SerializedProperty _state4Event;
        SerializedProperty _valueEvent;

        static GUIContent _textStartUp = new GUIContent("State 1 on start up");

        void OnEnable()
        {
            _stateCount = serializedObject.FindProperty("_stateCount");
            _value1 = serializedObject.FindProperty("_value1");
            _value2 = serializedObject.FindProperty("_value2");
            _value3 = serializedObject.FindProperty("_value3");
            _value4 = serializedObject.FindProperty("_value4");
            _interpolator = serializedObject.FindProperty("_interpolator");
            _sendOnStartUp = serializedObject.FindProperty("_sendOnStartUp");
            _state1Event = serializedObject.FindProperty("_state1Event");
            _state2Event = serializedObject.FindProperty("_state2Event");
            _state3Event = serializedObject.FindProperty("_state3Event");
            _state4Event = serializedObject.FindProperty("_state4Event");
            _valueEvent = serializedObject.FindProperty("_valueEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_stateCount);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_value1);
            EditorGUILayout.PropertyField(_value2);
            EditorGUILayout.PropertyField(_value3);
            EditorGUILayout.PropertyField(_value4);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_interpolator);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_sendOnStartUp, _textStartUp);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_state1Event);
            EditorGUILayout.PropertyField(_state2Event);
            EditorGUILayout.PropertyField(_state3Event);
            EditorGUILayout.PropertyField(_state4Event);
            EditorGUILayout.PropertyField(_valueEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
