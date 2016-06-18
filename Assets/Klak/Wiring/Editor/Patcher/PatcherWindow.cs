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

            window._patch = new Patch(patchInstance);
            window._patchManager.Select(window._patch);

            window.Show();
        }

        // Open from the main menu.
        [MenuItem("Window/Klak/Patcher")]
        static void Init()
        {
            EditorWindow.GetWindow<PatcherWindow>("Patcher").Show();
        }

        #endregion

        #region Wiring state class

        class WiringState
        {
            public Node node;
            public Inlet inlet;
            public Outlet outlet;

            public WiringState(Node node, Inlet inlet)
            {
                this.node = node;
                this.inlet = inlet;
            }

            public WiringState(Node node, Outlet outlet)
            {
                this.node = node;
                this.outlet = outlet;
            }
        }

        #endregion

        #region Private fields

        // Currently editing patch
        Patch _patch;

        // Helper objects
        PatchManager _patchManager;
        NodeFactory _nodeFactory;

        // Wiring state (null = not wiring now)
        WiringState _wiring;

        // Node property editor
        Editor _propertyEditor;

        // View size and positions
        Vector2 _mainViewSize;
        Vector2 _scrollMain;
        Vector2 _scrollSide;

        // Hierarchy change flag
        bool _hierarchyChanged;

        #endregion

        #region EditorWindow functions

        void OnEnable()
        {
            _patchManager = new PatchManager();
            _nodeFactory = new NodeFactory();
            _mainViewSize = Vector2.one * 300; // minimum view size

            _patchManager.Reset();
            _patch = _patchManager.RetrieveLastSelected();

            Undo.undoRedoPerformed += OnUndo;
            EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
        }

        void OnDisable()
        {
            if (_propertyEditor != null) {
                DestroyImmediate(_propertyEditor);
                _propertyEditor = null;
            }

            _patchManager = null;
            _nodeFactory = null;
            _patch = null;

            Undo.undoRedoPerformed -= OnUndo;
            EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
        }

        void OnUndo()
        {
            // We have to rescan patches and nodes,
            // because there may be an unknown ones.
            _patchManager.Reset();
            if (_patch != null && _patch.isValid) _patch.Rescan();

            // Manually update the GUI.
            Repaint();
        }

        void OnFocus()
        {
            // Do nothing if not yet enabled.
            if (_patchManager == null) return;

            // Rescan if there are changes in the hierarchy while unfocused.
            if (_hierarchyChanged) {
                _patchManager.Reset();
                if (_patch != null && _patch.isValid) _patch.Rescan();
            }

            // This is a good timing to reset the scroll view size.
            _mainViewSize = Vector2.one * 300; // minimum view size
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
                DrawPlaceholderGUI("Not available in play mode");
                return;
            }

            // If there is something wrong with the patch manager, reset it.
            if (!_patchManager.isValid) _patchManager.Reset();

            // Patch validity check.
            if (_patch != null)
                if (!_patch.isValid)
                    _patch = null; // Seems like not good. Abandon it.
                else if (!_patch.CheckNodesValidity())
                    _patch.Rescan(); // Some nodes are not good. Rescan them.

            // Get a patch if no one is selected.
            if (_patch == null)
                _patch = _patchManager.RetrieveLastSelected();

            // Draw a placeholder if no patch is available.
            // Disable GUI during the play mode, or when no patch is available.
            if ( _patch == null) {
                DrawPlaceholderGUI("No patch available");
                return;
            }

            // Tool bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // - Create node menu
            _nodeFactory.CreateNodeMenuGUI(_patch);
            GUILayout.Space(100);

            // - Patch selector
            var patchIndex = _patchManager.GetIndexOf(_patch);
            var newPatchIndex = EditorGUILayout.Popup(
                patchIndex, _patchManager.MakeNameList(),
                EditorStyles.toolbarDropDown
            );

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            // View area
            EditorGUILayout.BeginHorizontal();

            // - Main view
            DrawMainViewGUI();

            // - Side view (property editor)
            DrawSideBarGUI();

            EditorGUILayout.EndHorizontal();

            // Re-initialize the editor if the patch selection was changed.
            if (patchIndex != newPatchIndex)
            {
                _patch = _patchManager.RetrieveAt(newPatchIndex);
                _patchManager.Select(_patch);
                Repaint();
            }

            // Cancel wiring with a mouse click or hitting the esc key.
            if (_wiring != null)
            {
                var e = Event.current;
                if (e.type == EventType.MouseUp ||
                    (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape))
                {
                    _wiring = null;
                    e.Use();
                }
            }
        }

        #endregion

        #region Wiring functions

        // Go into the wiring state.
        void BeginWiring(object data)
        {
            _wiring = (WiringState)data;
        }

        // Remove a link between a pair of nodes.
        void RemoveLink(object data)
        {
            var link = (NodeLink)data;
            link.fromNode.RemoveLink(link.fromOutlet, link.toNode, link.toInlet);
        }

        // Draw the currently working link.
        void DrawWorkingLink()
        {
            var p1 = (Vector3)_wiring.node.windowPosition;
            var p2 = (Vector3)Event.current.mousePosition;

            if (_wiring.inlet != null)
            {
                // Draw a curve from the inlet button.
                p1 += (Vector3)_wiring.inlet.buttonRect.center;
                EditorUtility.DrawCurve(p2, p1);
            }
            else
            {
                // Draw a curve from the outlet button.
                p1 += (Vector3)_wiring.outlet.buttonRect.center;
                EditorUtility.DrawCurve(p1, p2);
            }

            // Request repaint continuously.
            Repaint();
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

        // Find and get the active node.
        Node GetActiveNode()
        {
            return _patch.nodeList.FirstOrDefault(n => n.isActive);
        }

        // Reset the internal state.
        void ResetState()
        {
            _patchManager.Reset();

            if (_patch == null || !_patch.isValid)
                _patch = _patchManager.RetrieveLastSelected();
            else
                _patch.Rescan();

            _mainViewSize = Vector2.one * 300; // minimum view size

            Repaint();
        }

        // Show the inlet/outlet context menu .
        void ShowNodeButtonMenu(Node node, Inlet inlet, Outlet outlet)
        {
            var menu = new GenericMenu();

            if (inlet != null)
            {
                // "New Connection"
                menu.AddItem(
                    new GUIContent("New Connection"), false,
                    BeginWiring, new WiringState(node, inlet)
                );

                // Disconnection items
                foreach (var targetNode in _patch.nodeList)
                {
                    var link = targetNode.TryGetLinkTo(node, inlet, _patch);
                    if (link == null) continue;

                    var label = "Disconnect/" + targetNode.displayName;
                    menu.AddItem(new GUIContent(label), false, RemoveLink, link);
                }
            }
            else
            {
                // "New Connection"
                menu.AddItem(
                    new GUIContent("New Connection"), false,
                    BeginWiring, new WiringState(node, outlet)
                );

                // Disconnection items
                foreach (var link in node.EnumerateLinksFrom(outlet, _patch))
                {
                    var label = "Disconnect/" + link.toNode.displayName;
                    menu.AddItem(new GUIContent(label), false, RemoveLink, link);
                }
            }

            menu.ShowAsContext();
        }

        // Process feedback from the leaf UI elemets.
        void ProcessUIFeedback(FeedbackQueue.RecordBase record)
        {
            // Delete request
            if (record is FeedbackQueue.DeleteNodeRecord)
            {
                var removeNode = ((FeedbackQueue.DeleteNodeRecord)record).node;

                // Remove related links.
                foreach (var node in _patch.nodeList)
                    node.RemoveLinksTo(removeNode, _patch);

                // Remove the node.
                removeNode.RemoveFromPatch(_patch);

                // Rescan the patch and repaint.
                _patch.Rescan();
                Repaint();
            }

            // Inlet button pressed
            if (record is FeedbackQueue.InletButtonRecord)
            {
                var info = (FeedbackQueue.InletButtonRecord)record;
                if (_wiring == null)
                    // Not in wiring: show the context menu.
                    ShowNodeButtonMenu(info.node, info.inlet, null);
                else
                    // Currently in wiring: try to make a link.
                    _wiring.node.TryLinkTo(_wiring.outlet, info.node, info.inlet);
            }

            // Outlet button pressed
            if (record is FeedbackQueue.OutletButtonRecord)
            {
                var info = (FeedbackQueue.OutletButtonRecord)record;
                if (_wiring == null)
                    // Not in wiring: show the context menu.
                    ShowNodeButtonMenu(info.node, null, info.outlet);
                else
                    // Currently in wiring: try to make a link.
                    info.node.TryLinkTo(info.outlet, _wiring.node, _wiring.inlet);
            }

            // Force to end wiring.
            _wiring = null;
        }

        // GUI function for the main view
        void DrawMainViewGUI()
        {
            FeedbackQueue.Reset();

            _scrollMain = EditorGUILayout.BeginScrollView(
                _scrollMain, false, false,
                GUIStyles.horizontalScrollbar,
                GUIStyles.verticalScrollbar,
                GUIStyles.background
            );

            // Draw the link lines.
            if (Event.current.type == EventType.Repaint)
                foreach (var node in _patch.nodeList)
                    if (!node.DrawLinkLines(_patch))
                    {
                        // Request repaint if position info is not ready.
                        Repaint();
                        break;
                    }

            // Draw all the nodes and make the bounding box.
            BeginWindows();
            foreach (var node in _patch.nodeList) {
                node.DrawWindowGUI();
                _mainViewSize = Vector2.Max(_mainViewSize, node.windowPosition);
            }
            EndWindows();

            // Place an empty box to expand the scroll view.
            GUILayout.Box(
                "", EditorStyles.label,
                GUILayout.Width(_mainViewSize.x + 256),
                GUILayout.Height(_mainViewSize.y + 128)
            );

            // Draw working link line while wiring.
            if (_wiring != null) DrawWorkingLink();

            EditorGUILayout.EndScrollView();

            // Process all the feedback from the leaf UI elements.
            while (!FeedbackQueue.IsEmpty)
                ProcessUIFeedback(FeedbackQueue.Dequeue());
        }

        // GUI function for the side bar
        void DrawSideBarGUI()
        {
            EditorGUIUtility.wideMode = true;

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(336));
            _scrollSide = EditorGUILayout.BeginScrollView(_scrollSide);

            // Determine the active node.
            var activeNode = GetActiveNode();

            // Destroy the previous property editor if it's not needed.
            if (_propertyEditor != null)
            {
                var targetNodeInstance = (Wiring.NodeBase)_propertyEditor.target;
                if (activeNode == null ||
                    !activeNode.IsRepresentationOf(targetNodeInstance))
                {
                    DestroyImmediate(_propertyEditor);
                    _propertyEditor = null;

                    // This is needed to clear the UnityEventDrawer cache.
                    EditorUtility.ClearPropertyDrawerCache();
                }
            }

            if (activeNode != null)
            {
                // Name field
                EditorGUILayout.LabelField("Node Attributes", EditorStyles.boldLabel);
                activeNode.DrawNameFieldGUI();
                EditorGUILayout.Space();

                // Show the property editor.
                EditorGUILayout.LabelField(activeNode.typeName + " Properties", EditorStyles.boldLabel);

                if (_propertyEditor == null)
                    _propertyEditor = activeNode.CreateEditor();

                _propertyEditor.OnInspectorGUI();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        // Draw empty placeholder GUI.
        void DrawPlaceholderGUI(string message)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(message, EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
