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
    [CustomEditor(typeof(EnvelopeGenerator))]
    public class EnvelopeGeneratorEditor : Editor
    {
        SerializedProperty _signalMode;
        SerializedProperty _attackTime;
        SerializedProperty _decayTime;
        SerializedProperty _sustainLevel;
        SerializedProperty _releaseTime;
        SerializedProperty _exponent;
        SerializedProperty _amplitude;
        SerializedProperty _bias;
        SerializedProperty _envelopeEvents;

        const int _vertexPerSegment = 20;
        Vector3[] _rectVertices;
        Vector3[] _lineVertices;

        void OnEnable()
        {
            _signalMode = serializedObject.FindProperty("_signalMode");
            _attackTime = serializedObject.FindProperty("_attackTime");
            _decayTime = serializedObject.FindProperty("_decayTime");
            _sustainLevel = serializedObject.FindProperty("_sustainLevel");
            _releaseTime = serializedObject.FindProperty("_releaseTime");
            _exponent = serializedObject.FindProperty("_exponent");
            _amplitude = serializedObject.FindProperty("_amplitude");
            _bias = serializedObject.FindProperty("_bias");
            _envelopeEvents = serializedObject.FindProperty("_envelopeEvents");

            _rectVertices = new Vector3[4];
            _lineVertices = new Vector3[_vertexPerSegment * 3 + 2];
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var showGateModeOptions =
                _signalMode.hasMultipleDifferentValues ||
                _signalMode.enumValueIndex == (int)EnvelopeGenerator.SignalMode.Gate;

            EditorGUILayout.PropertyField(_signalMode);
            EditorGUILayout.PropertyField(_attackTime);

            if (showGateModeOptions)
            {
                EditorGUILayout.PropertyField(_decayTime);
                EditorGUILayout.PropertyField(_sustainLevel);
            }

            EditorGUILayout.PropertyField(_releaseTime);
            EditorGUILayout.PropertyField(_exponent);

            EditorGUILayout.Space();

            if (!serializedObject.isEditingMultipleObjects)
            {
                DrawEnvelope((EnvelopeGenerator)target);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(_amplitude);
            EditorGUILayout.PropertyField(_bias);

            EditorGUILayout.Space();

            if (serializedObject.isEditingMultipleObjects)
                EditorGUILayout.PropertyField(_envelopeEvents, true);
            else
                DrawEventList();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawEventList()
        {
            var chlen = _envelopeEvents.arraySize;

            // FIXME: should be replaced with DelayedIntField in 5.3
            chlen = EditorGUILayout.IntField("Output Channels", chlen);

            // enlarge/shrink the list when the size is changed
            while (chlen > _envelopeEvents.arraySize)
                _envelopeEvents.InsertArrayElementAtIndex(_envelopeEvents.arraySize - 1);
            while (chlen < _envelopeEvents.arraySize)
                _envelopeEvents.DeleteArrayElementAtIndex(_envelopeEvents.arraySize - 1);

            for (var i = 0; i < _envelopeEvents.arraySize; i++)
            {
                var data = _envelopeEvents.GetArrayElementAtIndex(i);
                var label = new GUIContent("Channel " + i);
                EditorGUILayout.PropertyField(data, label);
            }
        }

        void DrawEnvelope(EnvelopeGenerator env)
        {
            var rect = GUILayoutUtility.GetRect(128, 64 * 3);

            // background
            _rectVertices[0] = new Vector3(rect.x, rect.y);
            _rectVertices[1] = new Vector3(rect.xMax, rect.y);
            _rectVertices[2] = new Vector3(rect.xMax, rect.yMax);
            _rectVertices[3] = new Vector3(rect.x, rect.yMax);

            Handles.DrawSolidRectangleWithOutline(
                _rectVertices, Color.white * 0.1f, Color.clear);

            // constants
            const int div = _vertexPerSegment;
            var exp = env.exponent;

            // segment lengths
            var Ta = env.attackTime;
            var Td = env.decayTime;
            var Ts = 0.2f;
            var Tr = env.releaseTime;

            // origin
            var vi = 0;
            var cutend = false;
            _lineVertices[vi++] = PointInRect(rect, 0, 0);

            // attack
            for (var i = 1; i < div && !cutend; i++)
            {
                var x = Ta * i / div;
                if (x > 1) { x = 1; cutend = true; }
                var y = Mathf.Pow((float)i / div, exp);
                _lineVertices[vi++] = PointInRect(rect, x, y);
            }

            // decay
            if (!cutend)
            for (var i = 0; i < div + 1 && !cutend; i++)
            {
                var x = Ta + Td * i / div;
                if (x > 1) { x = 1; cutend = true; }
                var y = Mathf.Pow(1.0f - (float)i / div, exp);
                y = Mathf.Lerp(y, 1.0f, env.sustainLevel);
                _lineVertices[vi++] = PointInRect(rect, x, y);
            }

            // release
            if (!cutend)
            for (var i = 0; i < div + 1 && !cutend; i++)
            {
                var x = Ta + Td + Ts + Tr * i / div;
                if (x > 1) { x = 1; cutend = true; }
                var y = Mathf.Pow(1.0f - (float)i / div, exp) * env.sustainLevel;
                _lineVertices[vi++] = PointInRect(rect, x, y);
            }

            for (; vi < _lineVertices.Length; vi++)
                _lineVertices[vi] = _lineVertices[vi - 1];

            // draw the line
            Handles.DrawAAPolyLine(_lineVertices);
        }

        Vector3 PointInRect(Rect rect, float x, float y)
        {
            return new Vector3(
                Mathf.Lerp(rect.x, rect.xMax, x),
                Mathf.Lerp(rect.y, rect.yMax, 1 - y), 0);
        }
    }
}
