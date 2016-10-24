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
                _selectionStack.Push(node.name);
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

        #region Customized GUI

        public override void NodeGUI(Graphs.Node node)
        {
            SelectNode(node);

            foreach (var slot in node.inputSlots)
                LayoutSlot(slot, slot.title, false, true, true, Graphs.Styles.triggerPinIn);

            node.NodeUI(this);

            foreach (var slot in node.outputSlots)
                LayoutSlot(slot, slot.title, true, false, true, Graphs.Styles.triggerPinOut);

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

            m_Host.EndWindows();

            // Graph edges
            edgeGUI.DoEdges();
            edgeGUI.DoDraggedEdge();

            // Mouse drag
            DragSelection(new Rect(-5000, -5000, 10000, 10000));

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
                NodeFactory.AddNodeItemsToMenu(menu, CreateMenuItemCallback);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Paste"), false, ContextMenuCallback, "Paste");
            }

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

            // Make it undo-able.
            Undo.RegisterCreatedObjectUndo(gameObject, "New Node");
        }

        #endregion
    }
}
