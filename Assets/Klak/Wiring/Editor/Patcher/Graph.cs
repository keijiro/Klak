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
            Clear(true);

            // Enumerate all the node instances.
            foreach (var i in _patch.GetComponentsInChildren<Wiring.NodeBase>())
                AddNode(Node.Create(i));

            // Enumerate all the edges.
            foreach (Node node in nodes)
                node.ScanSlots();
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
