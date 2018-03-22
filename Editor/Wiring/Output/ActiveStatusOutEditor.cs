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
using System.Linq;

namespace Klak.Wiring
{
    [CustomEditor(typeof(ActiveStatusOut))]
    public class ActiveStatusOutEditor : Editor
    {
        SerializedProperty _targetComponent;
        SerializedProperty _targetGameObject;

        // Component list cache and its parent game object
        string[] _componentList;
        GameObject _cachedGameObject;

        // Check if a given component is capable of being a target.
        bool IsTargetable(Component component)
        {
            return component.GetType().GetProperty("enabled") != null;
        }

        // Cache component of a given game object if it's
        // different from a previously given game object.
        void CacheComponentList(GameObject gameObject)
        {
            if (_cachedGameObject == gameObject) return;

            _componentList = gameObject.GetComponents<Component>().
                Where(x => IsTargetable(x)).Select(x => x.GetType().Name).ToArray();

            _cachedGameObject = gameObject;
        }

        void OnEnable()
        {
            _targetComponent = serializedObject.FindProperty("_targetComponent");
            _targetGameObject = serializedObject.FindProperty("_targetGameObject");
        }

        void OnDisable()
        {
            _targetComponent = null;
            _targetGameObject = null;

            _componentList = null;
            _cachedGameObject = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetComponent);

            // Show the component selector when a component is given.
            if (_targetComponent.objectReferenceValue != null)
            {
                // Cache the component list.
                var component = (Component)_targetComponent.objectReferenceValue;
                CacheComponentList(component.gameObject);

                if (_componentList.Length > 0)
                {
                    // Show the drop-down list.
                    var index = Array.IndexOf(_componentList, component.GetType().Name);
                    var newIndex = Mathf.Max(0, EditorGUILayout.Popup(" ", index, _componentList));

                    // Update the component if the selection was changed.
                    if (index != newIndex)
                        _targetComponent.objectReferenceValue =
                            component.GetComponent(_componentList[newIndex]);
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_targetGameObject);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
