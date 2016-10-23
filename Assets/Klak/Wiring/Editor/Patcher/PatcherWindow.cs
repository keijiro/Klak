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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Klak.Wiring.Patcher
{
    // Patcher window class
    public class PatcherWindow : EditorWindow
    {
        #region Exposed functions

        // Open the patcher window with a given patch.
        public static void OpenPatch(Wiring.Patch patchInstance)
        {
            var window = EditorWindow.GetWindow<PatcherWindow>("Patcher");
            window._graph = Graph.Create(patchInstance);
            window._graphGUI = window._graph.GetEditor();
            window.Show();
        }

        // Open from the main menu.
        [MenuItem("Window/Klak/Patcher")]
        static void Init()
        {
            EditorWindow.GetWindow<PatcherWindow>("Patcher").Show();
        }

        #endregion

        #region Private fields

        // Currently editing patch
        Graph _graph;
        GraphGUI _graphGUI;

        // Hierarchy change flag
        bool _hierarchyChanged;

        #endregion

        #region EditorWindow functions

        void OnUndo()
        {
            // We have to rescan patches and nodes,
            // because there may be an unknown ones.
            //if (_graph != null && _graph.isValid) _graph.RescanPatch();

            // Manually update the GUI.
            //Repaint();
        }

        void OnFocus()
        {
            // Rescan if there are changes in the hierarchy while unfocused.
            //if (_hierarchyChanged) {
            //    if (_graph != null && _graph.isValid) _graph.RescanPatch();
            //}
        }

        void OnLostFocus()
        {
            // To record hierarchy change while unfocused.
            _hierarchyChanged = false;
        }

        void OnHierarchyWindowChanged()
        {
            _hierarchyChanged = true;
        }

        void OnGUI()
        {
            // Do nothing while play mode.
            if (isPlayMode) {
                DrawPlaceholderGUI(
                    "Patcher is not available in play mode",
                    "You must exit play mode to resume editing."
                );
                return;
            }

            /*
            // Patch validity check.
            if (_graph != null)
                if (!_graph.isValid)
                    _graph = null; // Seems like not good. Abandon it.
                else if (!_graph.CheckNodesValidity())
                    _graph.RescanPatch(); // Some nodes are not good. Rescan them.
            */

            // Draw a placeholder if no patch is available.
            // Disable GUI during the play mode, or when no patch is available.
            if (_graphGUI == null) {
                DrawPlaceholderGUI(
                    "No patch is selected for editing",
                    "You must select a patch in Hierarchy, then press 'Open Patcher' from Inspector."
                );
                return;
            }

            // View area
            _graphGUI.BeginGraphGUI(this, new Rect(0, 0, position.width, position.height));
            _graphGUI.OnGraphGUI();
            _graphGUI.EndGraphGUI();
        }

        #endregion

        #region Private methods

        // Check if in the play mode.
        bool isPlayMode {
            get {
                return EditorApplication.isPlaying ||
                    EditorApplication.isPlayingOrWillChangePlaymode;
            }
        }

        // Draw empty placeholder GUI.
        void DrawPlaceholderGUI(string title, string comment)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title, EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(comment, EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
