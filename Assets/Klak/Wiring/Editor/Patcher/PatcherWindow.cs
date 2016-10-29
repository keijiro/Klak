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
    // Patcher window class
    public class PatcherWindow : EditorWindow
    {
        #region Class methods

        // Open the patcher window with a given patch.
        public static void OpenPatch(Wiring.Patch patch)
        {
            var window = EditorWindow.GetWindow<PatcherWindow>("Patcher");
            window.Initialize(patch);
            window.Show();
        }

        // Open from the main menu (only open the empty window).
        [MenuItem("Window/Klak/Patcher")]
        static void OpenEmpty()
        {
            OpenPatch(null);
        }

        #endregion

        #region EditorWindow functions

        void OnEnable()
        {
            // Initialize if it hasn't been initialized.
            // (this could be happened when a window layout is loaded)
            if (_graph == null) Initialize(null);

            Undo.undoRedoPerformed += OnUndo;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        void OnUndo()
        {
            // Invalidate the graph and force repaint.
            _graph.Invalidate();
            Repaint();
        }

        void OnFocus()
        {
            // Invalidate the graph if the hierarchy was touched while unfocused.
            if (_hierarchyChanged) _graph.Invalidate();
        }

        void OnLostFocus()
        {
            _hierarchyChanged = false;
        }

        void OnHierarchyChange()
        {
            _hierarchyChanged = true;
        }

        void OnGUI()
        {
            const float kBarHeight = 17;
            var width = position.width;
            var height = position.height;

            // Synchronize the graph with the patch at this point.
            if (!_graph.isValid)
            {
                _graphGUI.PushSelection();
                _graph.SyncWithPatch();
                _graphGUI.PopSelection();
            }

            // Show the placeholder if the patch is not available.
            if (!_graph.isValid)
            {
                DrawPlaceholderGUI();
                return;
            }

            // Main graph area
            _graphGUI.BeginGraphGUI(this, new Rect(0, 0, width, height - kBarHeight));
            _graphGUI.OnGraphGUI();
            _graphGUI.EndGraphGUI();

            // Clear selection on background click
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.clickCount == 1)
                _graphGUI.ClearSelection();

            // Status bar
            GUILayout.BeginArea(new Rect(0, height - kBarHeight, width, kBarHeight));
            GUILayout.Label(_graph.patch.name);
            GUILayout.EndArea();
        }

        #endregion

        #region Private members

        Graph _graph;
        GraphGUI _graphGUI;
        bool _hierarchyChanged;

        // Initializer (called from OpenPatch)
        void Initialize(Wiring.Patch patch)
        {
            hideFlags = HideFlags.HideAndDontSave;
            _graph = Graph.Create(patch);
            _graphGUI = _graph.GetEditor();
        }

        // Draw the placeholder GUI.
        void DrawPlaceholderGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("No patch is selected for editing", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("You must select a patch in Hierarchy, then press 'Open Patcher' from Inspector.", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
