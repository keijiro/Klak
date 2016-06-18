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
using System.Collections.ObjectModel;

namespace Klak.Wiring.Patcher
{
    // Editor representation of patch
    // It also manages mapping information between
    // node instances (NodeBase) and those editor representations.
    public class Patch
    {
        #region Public properties and methods

        // Validity check
        public bool isValid {
            get { return _instance != null; }
        }

        // Read-only node list
        public ReadOnlyCollection<Node> nodeList {
            get { return new ReadOnlyCollection<Node>(_nodeList); }
        }

        // Constructor
        public Patch(Wiring.Patch instance)
        {
            _instance = instance;
            _nodeList = new List<Node>();
            _instanceIDToNodeMap = new Dictionary<int, Node>();

            Rescan();
        }

        // Check if this is a representation of the given patch instance.
        public bool IsRepresentationOf(Wiring.Patch instance)
        {
            return _instance == instance;
        }

        // Get an editor representation of the given node.
        public Node GetNodeOfInstance(Wiring.NodeBase instance)
        {
            return _instanceIDToNodeMap[instance.GetInstanceID()];
        }

        // Rescan the patch.
        public void Rescan()
        {
            _nodeList.Clear();
            _instanceIDToNodeMap.Clear();

            // Enumerate all the node instances.
            foreach (var i in _instance.GetComponentsInChildren<Wiring.NodeBase>())
            {
                var node = new Node(i);
                _nodeList.Add(node);
                _instanceIDToNodeMap.Add(i.GetInstanceID(), node);
            }
        }

        // Check validity of all nodes in this patch.
        public bool CheckNodesValidity()
        {
            return !_nodeList.Exists(p => p == null);
        }

        // Add a node instance to the patch.
        public void AddNodeInstance(Wiring.NodeBase nodeInstance)
        {
            // Append to the hierarchy.
            nodeInstance.transform.parent = _instance.transform;

            // Register to this patch representation.
            var node = new Node(nodeInstance);
            _nodeList.Add(node);
            _instanceIDToNodeMap.Add(nodeInstance.GetInstanceID(), node);
        }

        #endregion

        #region Private members

        Wiring.Patch _instance;
        List<Node> _nodeList;
        Dictionary<int, Node> _instanceIDToNodeMap;

        #endregion
    }
}
