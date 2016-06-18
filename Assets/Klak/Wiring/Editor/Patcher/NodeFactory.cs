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
using System;

namespace Klak.Wiring.Patcher
{
    // Class for creating new nodes
    public class NodeFactory
    {
        #region Public methods

        // Constructor
        public NodeFactory()
        {
            EnumerateNodeTypes();
        }

        // Create and open the "Create New Node" dropdown menu.
        public void CreateNodeMenuGUI(Patch patch)
        {
            if (GUILayout.Button(_buttonText, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();

                foreach (var nodeType in _nodeTypes)
                    menu.AddItem(
                        new GUIContent(nodeType.label), false,
                        OnMenuItem, new MenuItemData(patch, nodeType.type)
                    );

                var oy = EditorStyles.toolbar.fixedHeight - 2;
                menu.DropDown(new Rect(1, oy, 1, 1));
            }
        }

        #endregion

        #region Menu item callback

        // Menu item data
        class MenuItemData
        {
            public Patch patch;
            public Type type;

            public MenuItemData(Patch patch, Type type)
            {
                this.patch = patch;
                this.type = type;
            }
        }

        // Menu item callback function
        void OnMenuItem(object userData)
        {
            var data = (MenuItemData)userData;

            // Create a game object.
            var name = ObjectNames.NicifyVariableName(data.type.Name);
            var gameObject = new GameObject(name);
            var instance = gameObject.AddComponent(data.type);

            // Add it to the patch.
            data.patch.AddNodeInstance((Wiring.NodeBase)instance);

            // Make it undo-able.
            Undo.RegisterCreatedObjectUndo(gameObject, "New Node");
        }

        #endregion

        #region Node type list

        class NodeType
        {
            public string label;
            public Type type;

            public NodeType(string label, Type type)
            {
                this.label = label;
                this.type = type;
            }
        }

        List<NodeType> _nodeTypes;

        // Enumerate all the node types.
        void EnumerateNodeTypes()
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
                    label = label.Substring(12);
                    _nodeTypes.Add(new NodeType(label, type));
                }
            }
        }

        #endregion

        #region Other private members

        static GUIContent _buttonText = new GUIContent("Create New Node");

        #endregion
    }
}
