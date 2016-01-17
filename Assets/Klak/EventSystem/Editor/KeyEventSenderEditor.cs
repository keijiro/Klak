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
    [CustomEditor(typeof(KeyEventSender))]
    public class KeyEventSenderEditor : Editor
    {
        SerializedProperty _eventType;
        SerializedProperty _keyCode;

        SerializedProperty _offValue;
        SerializedProperty _onValue;
        SerializedProperty _interpolator;

        SerializedProperty _triggerEvent;
        SerializedProperty _keyDownEvent;
        SerializedProperty _keyUpEvent;
        SerializedProperty _toggleOnEvent;
        SerializedProperty _toggleOffEvent;
        SerializedProperty _valueEvent;

        void OnEnable()
        {
            _eventType = serializedObject.FindProperty("_eventType");
            _keyCode = serializedObject.FindProperty("_keyCode");

            _offValue = serializedObject.FindProperty("_offValue");
            _onValue = serializedObject.FindProperty("_onValue");
            _interpolator = serializedObject.FindProperty("_interpolator");

            _triggerEvent = serializedObject.FindProperty("_triggerEvent");
            _keyDownEvent = serializedObject.FindProperty("_keyDownEvent");
            _keyUpEvent = serializedObject.FindProperty("_keyUpEvent");
            _toggleOnEvent = serializedObject.FindProperty("_toggleOnEvent");
            _toggleOffEvent = serializedObject.FindProperty("_toggleOffEvent");
            _valueEvent = serializedObject.FindProperty("_valueEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_eventType);
            EditorGUILayout.PropertyField(_keyCode);

            var showAllEvents = _eventType.hasMultipleDifferentValues;
            var eventType = (KeyEventSender.EventType)_eventType.enumValueIndex;

            if (showAllEvents || eventType == KeyEventSender.EventType.Value)
            {
                EditorGUILayout.PropertyField(_offValue);
                EditorGUILayout.PropertyField(_onValue);
                EditorGUILayout.PropertyField(_interpolator);
            }

            if (showAllEvents || eventType == KeyEventSender.EventType.Trigger)
                EditorGUILayout.PropertyField(_triggerEvent);

            if (showAllEvents || eventType == KeyEventSender.EventType.Gate)
            {
                EditorGUILayout.PropertyField(_keyDownEvent);
                EditorGUILayout.PropertyField(_keyUpEvent);
            }

            if (showAllEvents || eventType == KeyEventSender.EventType.Toggle)
            {
                EditorGUILayout.PropertyField(_toggleOnEvent);
                EditorGUILayout.PropertyField(_toggleOffEvent);
            }

            if (showAllEvents || eventType == KeyEventSender.EventType.Value)
                EditorGUILayout.PropertyField(_valueEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
