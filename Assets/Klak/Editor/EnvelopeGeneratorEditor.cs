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
        SerializedProperty _inputMode;
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

        static GUIContent _textOutput = new GUIContent("Output");

        void OnEnable()
        {
            _inputMode = serializedObject.FindProperty("_inputMode");
            _attackTime = serializedObject.FindProperty("_attackTime");
            _decayTime = serializedObject.FindProperty("_decayTime");
            _sustainLevel = serializedObject.FindProperty("_sustainLevel");
            _releaseTime = serializedObject.FindProperty("_releaseTime");
            _exponent = serializedObject.FindProperty("_exponent");
            _amplitude = serializedObject.FindProperty("_amplitude");
            _bias = serializedObject.FindProperty("_bias");
            _envelopeEvents = serializedObject.FindProperty("_envelopeEvents");

            _rectVertices = new Vector3[4];
            _lineVertices = new Vector3[_vertexPerSegment * 3 + 3];
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var showGateModeOptions =
                _inputMode.hasMultipleDifferentValues ||
                _inputMode.enumValueIndex == (int)EnvelopeGenerator.InputMode.Gate;

            EditorGUILayout.PropertyField(_inputMode);
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
            else if (showGateModeOptions)
                DrawEventList();
            else
            {
                var firstEvent = _envelopeEvents.GetArrayElementAtIndex(0);
                EditorGUILayout.PropertyField(firstEvent, _textOutput);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawEventList()
        {
            var chlen = _envelopeEvents.arraySize;

            // FIXME: should be replaced with DelayedIntField in 5.3
            chlen = EditorGUILayout.IntField("Output Channels", chlen);
            chlen = Mathf.Max(chlen, 1);

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
            var rect = GUILayoutUtility.GetRect(128, 64);

            // background
            _rectVertices[0] = new Vector3(rect.x, rect.y);
            _rectVertices[1] = new Vector3(rect.xMax, rect.y);
            _rectVertices[2] = new Vector3(rect.xMax, rect.yMax);
            _rectVertices[3] = new Vector3(rect.x, rect.yMax);

            Handles.DrawSolidRectangleWithOutline(
                _rectVertices, Color.white * 0.1f, Color.clear);

            // constants
            const int div = _vertexPerSegment;
            var gated = (env.signalMode == EnvelopeGenerator.InputMode.Gate);

            // segment lengths
            var Ta = env.attackTime;
            var Td = gated ? env.decayTime : 0.0f;
            var Ts = gated ? 0.2f : 0.0f;
            var Tr = env.releaseTime;

            // origin
            var vc = 0;
            _lineVertices[vc++] = PointInRect(rect, 0, 0);

            // attack
            for (var i = 1; i < div; i++)
            {
                var x = Mathf.Min(Ta * i / div, 1.0f);
                var y = env.GetLevelAtTime(x, Ta + Td + Ts);
                _lineVertices[vc++] = PointInRect(rect, x, y);
            }

            // decay
            if (gated)
            {
                for (var i = 0; i < div + 1; i++)
                {
                    var x = Mathf.Min(Ta + Td * i / div, 1.0f);
                    var y = env.GetLevelAtTime(x, Ta + Td + Ts);
                    _lineVertices[vc++] = PointInRect(rect, x, y);
                    if (x == 1.0f) break;
                }
            }

            // release
            if (Ta + Td + Ts <= 1)
            {
                for (var i = 0; i < div + 1; i++)
                {
                    var x = Mathf.Min(Ta + Td + Ts + Tr * i / div, 1.0f);
                    var y = env.GetLevelAtTime(x, Ta + Td + Ts);
                    _lineVertices[vc++] = PointInRect(rect, x, y);
                    if (x == 1.0f) break;
                }

                // zero flat line
                if (Ta + Td + Ts + Tr < 1)
                    _lineVertices[vc++] = PointInRect(rect, 1, 0);
            }
            else if (Ta + Td < 1)
            {
                // sustain level flat line
                var y = env.GetLevelAtTime(1, Ta + Td + Ts);
                _lineVertices[vc++] = PointInRect(rect, 1, y);
            }

            // draw the line
            Handles.DrawAAPolyLine(2.0f, vc, _lineVertices);
        }

        Vector3 PointInRect(Rect rect, float x, float y)
        {
            return new Vector3(
                Mathf.Lerp(rect.x, rect.xMax, x),
                Mathf.Lerp(rect.y, rect.yMax, 1 - y), 0);
        }
    }
}
