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
using UnityEngine.Events;
using UnityEditor;
using System;
using System.Reflection;
using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    // Spacialized node class
    public class Node : Graphs.Node
    {
        #region Public class methods

        // Factory method
        static public Node Create(Wiring.NodeBase runtimeInstance)
        {
            var node = CreateInstance<Node>();
            node.Initialize(runtimeInstance);
            return node;
        }

        #endregion

        #region Public member properties and methods

        // Runtime instance access
        public Wiring.NodeBase runtimeInstance {
            get { return _runtimeInstance; }
        }

        // Validity check
        public bool isValid {
            get { return _runtimeInstance != null; }
        }

        #endregion

        #region Overridden virtual methods

        // Node display title
        public override string title {
            get { return _runtimeInstance.name; }
        }

        // Removal from a graph
        public override void RemovingFromGraph()
        {
            if (graph != null && ((Graph)graph).isEditing)
                Undo.DestroyObjectImmediate(_runtimeInstance.gameObject);
        }

        // Dirty callback
        public override void Dirty()
        {
            base.Dirty();

            // Update serialized position info if it's changed.
            _serializedObject.Update();
            var spos = _serializedPosition.vector2Value;
            if (spos != position.position)
            {
                _serializedPosition.vector2Value = position.position;
                _serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Private members

        // Runtime instance of this node
        [NonSerialized] Wiring.NodeBase _runtimeInstance;

        // Serialized property accessor
        SerializedObject _serializedObject;
        SerializedProperty _serializedPosition;

        // Initializer (called from the Create method)
        void Initialize(Wiring.NodeBase runtimeInstance)
        {
            hideFlags = HideFlags.DontSave;

            // Object references
            _runtimeInstance = runtimeInstance;
            _serializedObject = new UnityEditor.SerializedObject(runtimeInstance);
            _serializedPosition = _serializedObject.FindProperty("_wiringNodePosition");

            // Basic information
            name = runtimeInstance.GetInstanceID().ToString();
            position = new Rect(_serializedPosition.vector2Value, Vector2.zero);

            // Slot initialization
            PopulateSlots();
        }

        // Convert all inlets/outlets into node slots.
        void PopulateSlots()
        {
            // Enumeration flags: all public and non-public members
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // Inlets (property)
            foreach (var prop in _runtimeInstance.GetType().GetProperties(flags))
            {
                // Check if it has an inlet attribute.
                var attrs = prop.GetCustomAttributes(typeof(Wiring.InletAttribute), true);
                if (attrs.Length == 0) continue;

                // Register the setter method as an input slot.
                var slot = AddInputSlot("set_" + prop.Name, prop.PropertyType);

                // Apply the standard nicifying rule.
                slot.title = ObjectNames.NicifyVariableName(prop.Name);
            }

            // Inlets (method)
            foreach (var method in _runtimeInstance.GetType().GetMethods(flags))
            {
                // Check if it has an inlet attribute.
                var attrs = method.GetCustomAttributes(typeof(Wiring.InletAttribute), true);
                if (attrs.Length == 0) continue;

                // Register the method as an input slot.
                var args = method.GetParameters();
                var dataType = args.Length > 0 ? args[0].ParameterType : null;
                var slot = AddInputSlot(method.Name, dataType);

                // Apply the standard nicifying rule.
                slot.title = ObjectNames.NicifyVariableName(method.Name);
            }

            // Outlets (UnityEvent members)
            foreach (var field in _runtimeInstance.GetType().GetFields(flags))
            {
                // Check if it has an outlet attribute.
                var attrs = field.GetCustomAttributes(typeof(Wiring.OutletAttribute), true);
                if (attrs.Length == 0) continue;

                // Register it as an output slot.
                var dataType = ConnectionTools.GetEventDataType(field.FieldType);
                var slot = AddOutputSlot(field.Name, dataType);

                // Apply the standard nicifying rule and remove tailing "Event".
                var title = ObjectNames.NicifyVariableName(field.Name);
                if (title.EndsWith(" Event")) title = title.Substring(0, title.Length - 6);
                slot.title = title;
            }
        }

        // Scan all inlets/outlets and populate edges.
        public void PopulateEdges()
        {
            // Enumeration flags: all public and non-public members
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var slot in outputSlots)
            {
                var field = _runtimeInstance.GetType().GetField(slot.name, flags);
                if (field == null) continue;

                var boundEvent = (UnityEventBase)field.GetValue(_runtimeInstance);
                var targetCount = boundEvent.GetPersistentEventCount();

                for (var i = 0; i < targetCount; i++)
                {
                    var target = boundEvent.GetPersistentTarget(i);

                    // Ignore it if it's a null event or the target is not a node.
                    if (target == null || !(target is Wiring.NodeBase)) continue;

                    // Try to retrieve the linked inlet.
                    var targetNode = graph[target.GetInstanceID().ToString()];
                    var methodName = boundEvent.GetPersistentMethodName(i);

                    if (targetNode != null)
                    {
                        var inlet = targetNode[methodName];
                        if (inlet != null) graph.Connect(slot, inlet);
                    }
                }
            }
        }

        #endregion
    }
}
