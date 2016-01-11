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
    // custom property drawer for MotionElement
    [CustomPropertyDrawer(typeof(ConstantMotion.MotionElement))]
    class ConstantMotionElementDrawer : PropertyDrawer
    {
        static GUIContent[] _basisLabels = {
            new GUIContent("Off"),
            new GUIContent("X Axis"),
            new GUIContent("Y Axis"),
            new GUIContent("Z Axis"),
            new GUIContent("Vector"),
            new GUIContent("Random")
        };

        static int[] _basisValues = {
            0,
            (int)ConstantMotion.MotionBasis.XAxis,
            (int)ConstantMotion.MotionBasis.YAxis,
            (int)ConstantMotion.MotionBasis.ZAxis,
            (int)ConstantMotion.MotionBasis.Vector,
            (int)ConstantMotion.MotionBasis.Random
        };

        static GUIContent _textSpeed = new GUIContent("Speed");
        static GUIContent _textRandomScale = new GUIContent("Random Scale");

        static int GetExpansionLevel(SerializedProperty property)
        {
            var basis = property.FindPropertyRelative("_basis");
            // fully expand if it has different values
            if (basis.hasMultipleDifferentValues) return 2;
            // "Off"
            if (basis.enumValueIndex == 0) return 0;
            // fully expand if it's in the vector mode
            if (basis.enumValueIndex == (int)ConstantMotion.MotionBasis.Vector) return 2;
            // expand just one level
            return 1;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var level = GetExpansionLevel(property);
            var rows = level == 0 ? 1 : 2 + level;
            return EditorGUIUtility.singleLineHeight * rows +
                   EditorGUIUtility.standardVerticalSpacing * (rows - 1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            var rowHeight =
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing;

            // motion basis selector drop-down
            var basis = property.FindPropertyRelative("_basis");
            EditorGUI.IntPopup(position, basis, _basisLabels, _basisValues, label);
            position.y += rowHeight;

            var level = GetExpansionLevel(property);
            if (level > 0)
            {
                // indent
                position.x += 16;
                position.width -= 16;
                EditorGUIUtility.labelWidth -= 16;

                if (level == 2)
                {
                    // vector box
                    var basisVector = property.FindPropertyRelative("_basisVector");
                    EditorGUI.PropertyField(position, basisVector, GUIContent.none);
                    position.y += rowHeight;
                }

                // speed box
                var speed = property.FindPropertyRelative("_speed");
                EditorGUI.PropertyField(position, speed, _textSpeed);
                position.y +=
                    EditorGUIUtility.singleLineHeight +
                    EditorGUIUtility.standardVerticalSpacing;

                // random scale
                var randomScale = property.FindPropertyRelative("_randomScale");
                EditorGUI.PropertyField(position, randomScale, _textRandomScale);
            }

            EditorGUI.EndProperty();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ConstantMotion))]
    public class ConstantMotionEditor : Editor
    {
        SerializedProperty _position;
        SerializedProperty _rotation;
        SerializedProperty _useLocalCoordinate;

        static GUIContent _textLocalCoordinate = new GUIContent("Local Coordinate");

        void OnEnable()
        {
            _position = serializedObject.FindProperty("_position");
            _rotation = serializedObject.FindProperty("_rotation");
            _useLocalCoordinate = serializedObject.FindProperty("_useLocalCoordinate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_position);
            EditorGUILayout.PropertyField(_rotation);
            EditorGUILayout.PropertyField(_useLocalCoordinate, _textLocalCoordinate);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
