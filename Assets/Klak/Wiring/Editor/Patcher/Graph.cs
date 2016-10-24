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

        // Is the graph is actually available?
        public bool isValid {
            get { return _patch != null; }
        }

        // Get source patch
        public Wiring.Patch patch {
            get { return _patch; }
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
            _patchInstanceID = patch != null ? patch.GetInstanceID() : 0;
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
                // Make this operation undoable.
                var fromNodeRuntime = ((Node)fromSlot.node).runtimeInstance;
                Undo.RecordObject(fromNodeRuntime, "Create Connection");

                // Add a serialized event.
                ConnectionTools.ConnectSlots(fromSlot, toSlot);

                // Send a repaint request to the inspector window because
                // the inspector is shown at this point in most cases.
                GUIUtility.RepaintAllInspectors();
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

                // Make this operation undoable.
                var fromNodeRuntime = ((Node)fromSlot.node).runtimeInstance;
                Undo.RecordObject(fromNodeRuntime, "Disconnect");

                // Remove the serialized event.
                ConnectionTools.DisconnectSlots(fromSlot, toSlot);
            }

            base.RemoveEdge(edge);
        }

        #endregion
    }
}
