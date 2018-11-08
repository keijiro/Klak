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
using System;
using System.Linq;
using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    // Specialized editor GUI class
    public class GraphGUI : Graphs.GraphGUI
    {
        #region Node selection stack

        // This is needed to keep selections over graph reconstruction (SyncWithPatch).
        // We don't care about edge selection.

        Stack<string> _selectionStack;

        public void PushSelection()
        {
            if (_selectionStack == null)
                _selectionStack = new Stack<string>();
            else
                _selectionStack.Clear();

            foreach (Node node in selection)
                if (node != null) _selectionStack.Push(node.name);
        }

        public void PopSelection()
        {
            selection.Clear();

            while (_selectionStack.Count > 0)
            {
                var found = graph.GetNodeByName(_selectionStack.Pop());
                if (found != null) selection.Add(found);
            }

            _selectionStack.Clear();

            UpdateUnitySelection();
        }

        #endregion

        #region Pasteboard support

        // This is a pretty hackish implementation. It exploits the standard
        // pasteboard APIs (unsupported) in a weird way.

        int _pasteOffset;

        // Copy
        protected override void CopyNodesToPasteboard()
        {
            // Do nothing if nothing is selected.
            if (selection.Count == 0) return;

            // Select the nodes in the scene hierarchy.
            Selection.objects = selection.Select(n => ((Node)n).runtimeInstance.gameObject).ToArray();

            // Copy them to the pasteboard.
            Unsupported.CopyGameObjectsToPasteboard();

            // Recover the selection.
            UpdateUnitySelection();

            // Reset the pasting offset counter.
            _pasteOffset = 1;
        }

        // Paste
        protected override void PasteNodesFromPasteboard()
        {
            var g = (Graph)graph;

            // Create a paste point. It has two level depth.
            var point1 = new GameObject("<PastePoint1>");
            var point2 = new GameObject("<PastePoint2>");
            point1.transform.parent = g.patch.transform;
            point2.transform.parent = point1.transform;

            // Select the paste point in the scene hierarchy.
            Selection.activeGameObject = point2;

            // Paste from the pasteboard.
            Unsupported.PasteGameObjectsFromPasteboard();

            // Point2 (placeholder) is not needed anymore.
            DestroyImmediate(point2);

            // Move pasted objects to the right position.
            var instances = point1.GetComponentsInChildren<Wiring.NodeBase>();
            foreach (var i in instances)
                i.transform.parent = g.patch.transform;

            // Point1 (group) is not needed anymore.
            DestroyImmediate(point1);

            // Resync the graph.
            g.Invalidate();
            g.SyncWithPatch();

            // Select and offset the pasted nodes.
            ClearSelection();
            foreach (var i in instances)
            {
                var node = graph[i.GetInstanceID().ToString()];
                node.position.position += Vector2.one * (_pasteOffset * kNodeGridSize * 2);
                node.Dirty();
                selection.Add(node);
            }
            _pasteOffset++;
        }

        // Duplicate
        protected override void DuplicateNodesThroughPasteboard()
        {
            // Do nothing if nothing is selected.
            if (selection.Count == 0) return;

            CopyNodesToPasteboard();
            PasteNodesFromPasteboard();
        }

        #endregion

        #region Customized GUI

        public override Graphs.IEdgeGUI edgeGUI {
            get {
                if (m_EdgeGUI == null)
                    m_EdgeGUI = new EdgeGUI { host = this };
                return m_EdgeGUI;
            }
        }

        public override void NodeGUI(Graphs.Node node)
        {
            SelectNode(node);

            foreach (var slot in node.inputSlots)
                LayoutSlot(slot, slot.title, false, true, true, Styles.pinIn);

            node.NodeUI(this);

            foreach (var slot in node.outputSlots)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                LayoutSlot(slot, slot.title, true, false, true, Styles.pinOut);
                EditorGUILayout.EndHorizontal();
            }

            DragNodes();
        }

        public override void OnGraphGUI()
        {
            // Show node subwindows.
            m_Host.BeginWindows();

            foreach (var node in graph.nodes)
            {
                // Recapture the variable for the delegate.
                var node2 = node;

                // Subwindow style (active/nonactive)
                var isActive = selection.Contains(node);
                var style = Graphs.Styles.GetNodeStyle(node.style, node.color, isActive);

                // Show the subwindow of this node.
                node.position = GUILayout.Window(
                    node.GetInstanceID(), node.position,
                    delegate { NodeGUI(node2); },
                    node.title, style, GUILayout.Width(150)
                );
            }

            // Workaround: If there is no node in the graph, put an empty
            // window to avoid corruption due to a bug.
            if (graph.nodes.Count == 0)
                GUILayout.Window(0, new Rect(0, 0, 1, 1), delegate {}, "", "MiniLabel");

            m_Host.EndWindows();

            // Graph edges
            edgeGUI.DoEdges();
            edgeGUI.DoDraggedEdge();

            // Mouse drag
        #if UNITY_2017_3_OR_NEWER
            DragSelection();
        #else
            DragSelection(new Rect(-5000, -5000, 10000, 10000));
        #endif

            // Context menu
            ShowCustomContextMenu();
            HandleMenuEvents();
        }

        #endregion

        #region Customized context menu

		void ShowCustomContextMenu()
		{
            // Only cares about single right click.
			if (Event.current.type != EventType.MouseDown) return;
            if (Event.current.button != 1) return;
            if (Event.current.clickCount != 1) return;

            // Consume this mouse event.
			Event.current.Use();

            // Record the current mouse position
			m_contextMenuMouseDownPosition = Event.current.mousePosition;

            // Build a context menu.
            var menu = new GenericMenu();

			if (selection.Count != 0)
			{
                // Node operations
                menu.AddItem(new GUIContent("Cut"), false, ContextMenuCallback, "Cut");
                menu.AddItem(new GUIContent("Copy"), false, ContextMenuCallback, "Copy");
                menu.AddItem(new GUIContent("Duplicate"), false, ContextMenuCallback, "Duplicate");
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, ContextMenuCallback, "Delete");
			}
			else if (edgeGUI.edgeSelection.Count != 0)
            {
                // Edge operations
                menu.AddItem(new GUIContent("Delete"), false, ContextMenuCallback, "Delete");
            }
            else
            {
                // Clicked on empty space.
                menu.AddItem(new GUIContent("Paste"), false, ContextMenuCallback, "Paste");
            }

            // "Create" menu
            menu.AddSeparator("");
            NodeFactory.AddNodeItemsToMenu(menu, CreateMenuItemCallback);

            menu.ShowAsContext();
		}

        void ContextMenuCallback(object data)
        {
            m_Host.SendEvent(EditorGUIUtility.CommandEvent((string)data));
        }

        void CreateMenuItemCallback(object data)
        {
            var type = data as Type;

            // Create a game object.
            var name = ObjectNames.NicifyVariableName(type.Name);
            var gameObject = new GameObject(name);
            var nodeRuntime = (Wiring.NodeBase)gameObject.AddComponent(type);
            gameObject.transform.parent = ((Graph)graph).patch.transform;

            // Add it to the graph.
            var node = Node.Create(nodeRuntime);
            node.position = new Rect((Vector2)m_contextMenuMouseDownPosition, Vector2.zero);
            node.Dirty();
            graph.AddNode(node);

            // Select the new node.
            ClearSelection();
            selection.Add(node);
            UpdateUnitySelection();

            // Make it undo-able.
            Undo.RegisterCreatedObjectUndo(gameObject, "New Node");
        }

        #endregion
    }
}
