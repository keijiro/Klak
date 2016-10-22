using UnityEngine;
using UnityEditor;

using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    public class Graph : Graphs.Graph
    {
        public GraphGUI GetEditor()
        {
            var gui = CreateInstance<GraphGUI>();
            gui.graph = this;
            gui.hideFlags = HideFlags.HideAndDontSave;
            return gui;
        }

        Wiring.Patch _patch;
        bool _editing;

        static public Graph CreateFromPatch(Wiring.Patch patch)
        {
            var graph = CreateInstance<Graph>();
            graph.hideFlags = HideFlags.HideAndDontSave;

            graph._patch = patch;
            graph.RescanPatch();

            return graph;
        }

        public bool IsRepresentationOf(Wiring.Patch patch)
        {
            return _patch.GetInstanceID() == patch.GetInstanceID();
        }

        public void RescanPatch()
        {
            _editing = false;

            Clear(true);

            // Enumerate all the node instances.
            foreach (var i in _patch.GetComponentsInChildren<Wiring.NodeBase>())
                AddNode(Node.Create(i));

            // Enumerate all the edges.
            foreach (Node node in nodes)
                node.PopulateEdges();

            _editing = true;
        }

        public bool isValid {
            get { return _patch != null; }
        }

        public bool CheckNodesValidity()
        {
            foreach (Node node in nodes)
                if (!node.isValid) return false;
            return true;
        }

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

            if (_editing)
            {
                Undo.RecordObject(((Node)fromSlot.node).runtimeInstance, "Link To Node");

                LinkUtility.TryLinkNodes(
                    ((Node)fromSlot.node).runtimeInstance,
                    LinkUtility.GetEventOfOutputSlot(fromSlot),
                    ((Node)toSlot.node).runtimeInstance,
                    LinkUtility.GetMethodOfInputSlot(toSlot)
                );

                EditorUtility.RepaintAllInspectors();
            }

            return edge;
        }

        // Remove a connection between slots.
        public override void RemoveEdge(Graphs.Edge edge)
        {
            var fromSlot = edge.fromSlot;
            var toSlot = edge.toSlot;

            LinkUtility.RemoveLinkNodes(
                ((Node)fromSlot.node).runtimeInstance,
                LinkUtility.GetEventOfOutputSlot(fromSlot),
                ((Node)toSlot.node).runtimeInstance,
                LinkUtility.GetMethodOfInputSlot(toSlot)
            );

            base.RemoveEdge(edge);

            EditorUtility.RepaintAllInspectors();
        }

        #endregion
    }

    public class GraphGUI : Graphs.GraphGUI
    {
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
