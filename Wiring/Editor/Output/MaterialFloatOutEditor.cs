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
using System;
using System.Collections.Generic;

namespace Klak.Wiring
{
    [CustomEditor(typeof(MaterialFloatOut))]
    public class MaterialFloatOutEditor : Editor
    {
        SerializedProperty _target;
        SerializedProperty _propertyName;

        string[] _propertyList; // cached property list
        Shader _cachedShader;   // shader used to cache the list

        void OnEnable()
        {
            _target = serializedObject.FindProperty("_target");
            _propertyName = serializedObject.FindProperty("_propertyName");
        }

        void OnDisable()
        {
            _target = null;
            _propertyName = null;
            _propertyList = null;
            _cachedShader = null;
        }

        // Retrieve shader from a target renderer.
        Shader RetrieveTargetShader(UnityEngine.Object target)
        {
            var renderer = target as Renderer;
            if (renderer == null) return null;

            var material = renderer.sharedMaterial;
            if (material == null) return null;

            return material.shader;
        }

        // Cache properties of a given shader if it's
        // different from a previously given one.
        void CachePropertyList(Shader shader)
        {
            if (_cachedShader == shader) return;

            var temp = new List<string>();

            var count = ShaderUtil.GetPropertyCount(shader);
            for (var i = 0; i < count; i++)
            {
                var propType = ShaderUtil.GetPropertyType(shader, i);
                if (propType == ShaderUtil.ShaderPropertyType.Float ||
                    propType == ShaderUtil.ShaderPropertyType.Range)
                    temp.Add(ShaderUtil.GetPropertyName(shader, i));
            }

            _propertyList = temp.ToArray();
            _cachedShader = shader;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_target);

            // Try to retrieve the target shader.
            var shader = RetrieveTargetShader(_target.objectReferenceValue);

            if (shader != null)
            {
                // Cache the property list of the target shader.
                CachePropertyList(shader);

                // If there are suitable candidates...
                if (_propertyList.Length > 0)
                {
                    // Show the drop-down list.
                    var index = Array.IndexOf(_propertyList, _propertyName.stringValue);
                    var newIndex = EditorGUILayout.Popup("Property", index, _propertyList);

                    // Update the property if the selection was changed.
                    if (index != newIndex)
                        _propertyName.stringValue = _propertyList[newIndex];
                }
                else
                    _propertyName.stringValue = ""; // reset on failure
            }
            else
                _propertyName.stringValue = ""; // reset on failure

            serializedObject.ApplyModifiedProperties();
        }
    }
}
