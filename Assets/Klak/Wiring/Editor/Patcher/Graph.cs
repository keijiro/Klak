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

using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    // Specialized graph class
    public class Graph : Graphs.Graph
    {
        #region Public class methods

        // Factory method
        static public Graph Create(Wiring.Patch runtimeInstance)
        {
            var graph = CreateInstance<Graph>();
            graph.hideFlags = HideFlags.HideAndDontSave;
            graph.Initialize(runtimeInstance);
            return graph;
        }

        #endregion

        #region Public member properties and methods

        // Check if this graph is still editable.
        public bool isValid {
            get { return _runtimeInstance != null; }
        }

        // Check if nodes in this graph are still alive.
        public bool CheckNodesValidity()
        {
            foreach (Node node in nodes)
                if (!node.isValid) return false;
            return true;
        }

        // Check if this graph is a representation of a given patch.
        public bool IsRepresentationOf(Wiring.Patch patch)
        {
            return _runtimeInstance.GetInstanceID() == patch.GetInstanceID();
        }

        // Create a specialized editor GUI for this graph.
        public GraphGUI GetEditor()
        {
            var gui = CreateInstance<GraphGUI>();
            gui.graph = this;
            gui.hideFlags = HideFlags.HideAndDontSave;
            return gui;
        }

        // Rescan the source patch and reset the internal state.
        public void RescanPatch()
        {
            _ready = false;

            Clear(true);

            // Enumerate all the node instances.
            foreach (var i in _runtimeInstance.GetComponentsInChildren<Wiring.NodeBase>())
                AddNode(Node.Create(i));

            // Enumerate all the edges.
            foreach (Node node in nodes)
                node.PopulateEdges();

            _ready = true;
        }

        #endregion

        #region Private members

        Wiring.Patch _runtimeInstance;
        bool _ready;

        // Initializer (called from the Create method)
        void Initialize(Wiring.Patch runtimeInstance)
        {
            _runtimeInstance = runtimeInstance;
            RescanPatch();
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

            if (_ready)
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

            base.RemoveEdge(edge);
        }

        #endregion
    }

    // Specialized editor GUI class
    public class GraphGUI : Graphs.GraphGUI
    {
        // Slightly customized node GUI
        public override void NodeGUI(Graphs.Node node)
        {
            GUILayout.BeginVertical(GUILayout.Width(150));

            SelectNode(node);

            foreach (var slot in node.inputSlots)
                LayoutSlot(slot, slot.title, false, true, true, Graphs.Styles.triggerPinIn);

            node.NodeUI(this);

            foreach (var slot in node.outputSlots)
                LayoutSlot(slot, slot.title, true, false, true, Graphs.Styles.triggerPinOut);

            DragNodes();

            GUILayout.EndVertical();
        }
    }
}
