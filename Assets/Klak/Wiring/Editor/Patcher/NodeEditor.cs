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

namespace Klak.Wiring.Patcher
{
    // Inspector GUI for the specialized node
    [CustomEditor(typeof(Node))]
    class NodeEditor : Editor
    {
        // Node component editor
        Editor _editor;

        void OnEnable()
        {
            if (_editor == null)
                _editor = CreateEditor(((Node)target).runtimeInstance);
        }

        void OnDestroy()
        {
            DestroyImmediate(_editor);
        }

        protected override void OnHeaderGUI()
        {
            if (_editor == null) return;

            EditorGUILayout.Space();

            // Retrieve the header title (type name).
            var instance = ((Node)target).runtimeInstance;
            var title = ObjectNames.NicifyVariableName(instance.GetType().Name);

            // Show the header title.
            GUILayout.BeginHorizontal();
            GUILayout.Space(14);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        public override void OnInspectorGUI()
        {
            if (_editor == null) return;

            // Show the node name field.
            var instance = ((Node)target).runtimeInstance;
            instance.name = EditorGUILayout.TextField("Name", instance.name);

            EditorGUILayout.Space();

            // Node properties
            _editor.OnInspectorGUI();
        }
    }
}
