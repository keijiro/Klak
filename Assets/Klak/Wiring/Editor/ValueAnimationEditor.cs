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
    [CustomEditor(typeof(ValueAnimation))]
    public class ValueAnimationEditor : Editor
    {
        SerializedProperty _animations;
        SerializedProperty _playOnStart;
        SerializedProperty _speed;
        SerializedProperty _valueEvent;

        void OnEnable()
        {
            _animations = serializedObject.FindProperty("_animations");
            _playOnStart = serializedObject.FindProperty("_playOnStart");
            _speed = serializedObject.FindProperty("_speed");
            _valueEvent = serializedObject.FindProperty("_valueEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShowAnimationList(_animations);

            EditorGUILayout.PropertyField(_playOnStart);
            EditorGUILayout.PropertyField(_speed);
            EditorGUILayout.PropertyField(_valueEvent);

            serializedObject.ApplyModifiedProperties();
        }

        void ShowAnimationList(SerializedProperty animations)
        {
            var count = animations.arraySize;

            // FIXME: should be replaced with DelayedIntField in 5.3
            count = EditorGUILayout.IntField("Animation Count", count);
            count = Mathf.Max(count, 1);

            // enlarge/shrink the list when the size is changed
            while (count > animations.arraySize)
                animations.InsertArrayElementAtIndex(animations.arraySize - 1);
            while (count < animations.arraySize)
                animations.DeleteArrayElementAtIndex(animations.arraySize - 1);

            EditorGUI.indentLevel++;

            for (var i = 0; i < animations.arraySize; i++)
            {
                var data = animations.GetArrayElementAtIndex(i);
                var label_i = new GUIContent("Animation " + i);
                EditorGUILayout.PropertyField(data, label_i);
            }

            EditorGUI.indentLevel--;
        }
    }
}
