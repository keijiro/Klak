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
using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    // Specialized graph class
    public class Graph : Graphs.Graph
    {
        #region Public class methods

        // Factory method
        static public Graph Create(Wiring.Patch patch)
        {
            var graph = CreateInstance<Graph>();
            graph.Initialize(patch);
            return graph;
        }

        #endregion

        #region Public member properties and methods

        // Current editing state
        public bool isEditing {
            get { return _isEditing; }
        }

        // Get source patch
        public Wiring.Patch patch {
            get {
                if (_patch == null) ForceSyncNow();
                return _patch;
            }
        }

        // Create a specialized editor GUI for this graph.
        public GraphGUI GetEditor()
        {
            var gui = CreateInstance<GraphGUI>();
            gui.graph = this;
            gui.hideFlags = HideFlags.HideAndDontSave;
            return gui;
        }

        // Invalidate the internal state.
        public void Invalidate()
        {
            _patch = null;
        }

        // Synchronize with the source patch.
        public void SyncWithPatch()
        {
            if (_patch == null) ForceSyncNow();
        }

        #endregion

        #region Private members

        [NonSerialized] Wiring.Patch _patch;
        int _patchInstanceID;
        bool _isEditing;

        // Initializer (called from the Create method)
        void Initialize(Wiring.Patch patch)
        {
            hideFlags = HideFlags.HideAndDontSave;
            _patchInstanceID = patch.GetInstanceID();
        }

        // Synchronize with the source patch immediately.
        void ForceSyncNow()
        {
            // Operations on this graph are not reflected to the patch from now.
            _isEditing = false;

            // Reset the state.
            Clear(true);
            _patch = null;

            // Retrieve the patch object based on the instance ID.
            foreach (var obj in GameObject.FindObjectsOfType<Wiring.Patch>())
            {
                if (obj.GetInstanceID() == _patchInstanceID)
                {
                    _patch = obj;
                    break;
                }
            }

            // Scan the patch if available.
            if (_patch != null)
            {
                // Enumerate all the node instances.
                foreach (var i in _patch.GetComponentsInChildren<Wiring.NodeBase>())
                    AddNode(Node.Create(i));

                // Enumerate all the edges.
                foreach (Node node in nodes)
                    node.PopulateEdges();
            }

            // Operations will be reflected to the patch again.
            _isEditing = true;
        }

        #endregion

        #region Overridden virtual methods

        // Check if slots can be connected.
        public override bool CanConnect(Graphs.Slot fromSlot, Graphs.Slot toSlot)
        {
            // If the outlet is bang, any inlet can be connected.
            if (fromSlot.dataType == null) return true;
            // Apply simple type matching.
            return fromSlot.dataType == toSlot.dataType;
        }

        // Establish a connection between slots.
        public override Graphs.Edge Connect(Graphs.Slot fromSlot, Graphs.Slot toSlot)
        {
            var edge = base.Connect(fromSlot, toSlot);

            if (_isEditing)
            {
                var fromNodeRuntime = ((Node)fromSlot.node).runtimeInstance;
                var toNodeRuntime = ((Node)toSlot.node).runtimeInstance;

                // Make this operation undoable.
                Undo.RecordObject(fromNodeRuntime, "Link To Node");

                // Add a serialized event.
                LinkUtility.TryLinkNodes(
                    fromNodeRuntime, LinkUtility.GetEventOfOutputSlot(fromSlot),
                    toNodeRuntime, LinkUtility.GetMethodOfInputSlot(toSlot)
                );

                // Send a repaint request to the inspector window because
                // the inspector is shown at this point in most cases.
                EditorUtility.RepaintAllInspectors();
            }

            return edge;
        }

        // Remove a connection between slots.
        public override void RemoveEdge(Graphs.Edge edge)
        {
            if (_isEditing)
            {
                var fromSlot = edge.fromSlot;
                var toSlot = edge.toSlot;

                var fromNodeRuntime = ((Node)fromSlot.node).runtimeInstance;
                var toNodeRuntime = ((Node)toSlot.node).runtimeInstance;

                // Make this operation undoable.
                Undo.RecordObject(fromNodeRuntime, "Remove Link");

                // Remove the serialized event.
                LinkUtility.RemoveLinkNodes(
                    fromNodeRuntime, LinkUtility.GetEventOfOutputSlot(fromSlot),
                    toNodeRuntime, LinkUtility.GetMethodOfInputSlot(toSlot)
                );
            }

            base.RemoveEdge(edge);
        }

        #endregion
    }

    // Specialized editor GUI class
    public class GraphGUI : Graphs.GraphGUI
    {
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
