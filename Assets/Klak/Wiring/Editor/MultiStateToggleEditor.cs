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
    [CustomEditor(typeof(MultiStateToggle))]
    public class MultiStateToggleEditor : Editor
    {
        SerializedProperty _eventType;
        SerializedProperty _interpolator;
        SerializedProperty _triggerEvents;
        SerializedProperty _stateValues;
        SerializedProperty _valueEvent;

        void OnEnable()
        {
            _eventType = serializedObject.FindProperty("_eventType");
            _interpolator = serializedObject.FindProperty("_interpolator");
            _triggerEvents = serializedObject.FindProperty("_triggerEvents");
            _stateValues = serializedObject.FindProperty("_stateValues");
            _valueEvent = serializedObject.FindProperty("_valueEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Receives events with the Toggle method.",
                MessageType.None);

            EditorGUILayout.PropertyField(_eventType);

            var showAllEvents = _eventType.hasMultipleDifferentValues;
            var eventType = (MultiStateToggle.EventType)_eventType.enumValueIndex;

            if (showAllEvents || eventType == MultiStateToggle.EventType.Value)
                EditorGUILayout.PropertyField(_interpolator);

            if (showAllEvents || eventType == MultiStateToggle.EventType.Trigger)
                ShowStateList(_triggerEvents, "Trigger");

            if (showAllEvents || eventType == MultiStateToggle.EventType.Value)
            {
                ShowStateList(_stateValues, "Value");
                EditorGUILayout.PropertyField(_valueEvent);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void ShowStateList(SerializedProperty stateList, string label)
        {
            var count = stateList.arraySize;

            // FIXME: should be replaced with DelayedIntField in 5.3
            count = EditorGUILayout.IntField("State Count", count);
            count = Mathf.Max(count, 1);

            // enlarge/shrink the list when the size is changed
            while (count > stateList.arraySize)
                stateList.InsertArrayElementAtIndex(stateList.arraySize - 1);
            while (count < stateList.arraySize)
                stateList.DeleteArrayElementAtIndex(stateList.arraySize - 1);

            EditorGUI.indentLevel ++;

            for (var i = 0; i < stateList.arraySize; i++)
            {
                var data = stateList.GetArrayElementAtIndex(i);
                var label_i = new GUIContent(label + " " + i);
                EditorGUILayout.PropertyField(data, label_i);
            }

            EditorGUI.indentLevel --;
        }
    }
}
