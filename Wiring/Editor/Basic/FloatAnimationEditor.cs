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
    [CustomEditor(typeof(FloatAnimation))]
    public class FloatAnimationEditor : Editor
    {
        SerializedProperty _curve;
        SerializedProperty _speed;
        SerializedProperty _playOnStart;
        SerializedProperty _floatEvent;

        void OnEnable()
        {
            _curve = serializedObject.FindProperty("_curve");
            _speed = serializedObject.FindProperty("_speed");
            _playOnStart = serializedObject.FindProperty("_playOnStart");
            _floatEvent = serializedObject.FindProperty("_floatEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_curve);
            EditorGUILayout.PropertyField(_speed);
            EditorGUILayout.PropertyField(_playOnStart);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_floatEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
