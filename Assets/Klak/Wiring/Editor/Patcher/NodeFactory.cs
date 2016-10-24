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

namespace Klak.Wiring.Patcher
{
    // Class for creating new nodes
    public static class NodeFactory
    {
        #region Public methods

        // Add menu items to a given menu.
        public static void AddNodeItemsToMenu(GenericMenu menu, GenericMenu.MenuFunction2 callback)
        {
            if (_nodeTypes == null) EnumerateNodeTypes();

            foreach (var nodeType in _nodeTypes)
                menu.AddItem(nodeType.label, false, callback, nodeType.type);
        }

        #endregion

        #region Node type list

        class NodeType
        {
            public GUIContent label;
            public Type type;

            public NodeType(string label, Type type)
            {
                this.label = new GUIContent(label);
                this.type = type;
            }
        }

        static List<NodeType> _nodeTypes;

        // Enumerate all the node types.
        static void EnumerateNodeTypes()
        {
            _nodeTypes = new List<NodeType>();

            // Scan all assemblies in the current domain.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Scan all types in the assembly.
                foreach(var type in assembly.GetTypes())
                {
                    // Retrieve AddComponentMenu attributes.
                    var attr = type.GetCustomAttributes(typeof(AddComponentMenu), true);
                    if (attr.Length == 0) continue;

                    // Retrieve the menu label.
                    var label = ((AddComponentMenu)attr[0]).componentMenu;

                    // Chech if it's in the Wiring menu.
                    if (!label.StartsWith("Klak/Wiring/")) continue;

                    // Add this to the node type list.
                    label = "Create/" + label.Substring(12);
                    _nodeTypes.Add(new NodeType(label, type));
                }
            }
        }

        #endregion
    }
}
