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
using UnityEditor.Events;
using System.Collections.Generic;
using System.Reflection;

using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    public class Node : Graphs.Node
    {
        #region Public class methods

        // Factory method
        static public Node Create(Wiring.NodeBase runtimeInstance)
        {
            var node = CreateInstance<Node>();
            node.hideFlags = HideFlags.DontSave;
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
            Undo.DestroyObjectImmediate(_runtimeInstance.gameObject);
        }

        #endregion

        #region Private members

        // Runtime instance of this node
        Wiring.NodeBase _runtimeInstance;

        // Serialized property accessor
        SerializedObject _serializedObject;
        SerializedProperty _serializedPosition;

        // Initializer (called from the Create method)
        void Initialize(Wiring.NodeBase runtimeInstance)
        {
            // Object references
            _runtimeInstance = runtimeInstance;
            _serializedObject = new UnityEditor.SerializedObject(runtimeInstance);
            _serializedPosition = _serializedObject.FindProperty("_wiringNodePosition");

            // Basic information
            name = runtimeInstance.GetInstanceID().ToString();

            // Window initialization
            InitializePosition();

            // Slot initialization
            PopulateSlots();
        }

        // Restore/Initialize position
        void InitializePosition()
        {
            var position = _serializedPosition.vector2Value;
            if (position == Wiring.NodeBase.uninitializedNodePosition)
            {
                // Serialize the node position.
                _serializedPosition.vector2Value = this.position.position;
                _serializedObject.ApplyModifiedProperties();
            }
            else
            {
                // Use the serialized window position.
                this.position.position = position;
            }
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
                var dataType = LinkUtility.GetEventDataType(field.FieldType);
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

    [CustomEditor(typeof(Node))]
    class NodeEditor : Editor
    {
        Editor _editor;

        void OnEnable()
        {
            if (_editor == null)
                _editor = CreateEditor(((Node)target).runtimeInstance);
        }

        void OnDisable()
        {
            // This is needed to clear the UnityEventDrawer cache.
            EditorUtility.ClearPropertyDrawerCache();
        }

        void OnDestroy()
        {
            DestroyImmediate(_editor);
        }

        protected override void OnHeaderGUI()
        {
            EditorGUILayout.Space();

            // Retrieve the header title (type name).
            var instance = ((Node)target).runtimeInstance;
            var title = ObjectNames.NicifyVariableName(instance.GetType().Name);

            // Show the header title.
            GUILayout.BeginHorizontal();
            GUILayout.Space(14);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        public override void OnInspectorGUI()
        {
            // Show the node name field.
            var instance = ((Node)target).runtimeInstance;
            instance.name = EditorGUILayout.TextField("Name", instance.name);

            EditorGUILayout.Space();

            // Node properties
            _editor.OnInspectorGUI();
        }
    }

    /*
        // Remove itself from the patch.
        public void RemoveFromPatch(Patch patch)
        {
            Undo.DestroyObjectImmediate(_instance.gameObject);
        }

        // Enumerate all the links from a given outlet.
        public NodeLink[] EnumerateLinksFrom(Outlet outlet, Patch patch)
        {
            if (_cachedLinks == null) CacheLinks(patch);

            var temp = new List<NodeLink>();

            foreach (var link in _cachedLinks)
                if (link.fromOutlet == outlet) temp.Add(link);

            return temp.ToArray();
        }

        // If this node has a link to a given inlet, return it.
        public NodeLink TryGetLinkTo(Node targetNode, Inlet inlet, Patch patch)
        {
            if (_cachedLinks == null) CacheLinks(patch);

            foreach (var link in _cachedLinks)
                if (link.toInlet == inlet) return link;

            return null;
        }

        // Try to make a link from the outlet to a given node/inlet.
        public void TryLinkTo(Outlet outlet, Node targetNode, Inlet inlet)
        {
            Undo.RecordObject(_instance, "Link To Node");

            // Retrieve the target method (inlet) information.
            var targetMethod = targetNode._instance.GetType().GetMethod(inlet.methodName);

            // Try to create a link.
            var result = LinkUtility.TryLinkNodes(
                _instance, outlet.boundEvent,
                targetNode._instance, targetMethod
            );

            // Clear the cache and update information.
            if (result) {
                _cachedLinks = null;
                _serializedObject.Update();
            }
        }

        // Remove a link to a given node/inlet.
        public void RemoveLink(Outlet outlet, Node targetNode, Inlet inlet)
        {
            Undo.RecordObject(_instance, "Remove Link");

            // Retrieve the target method (inlet) information.
            var targetMethod = targetNode._instance.GetType().GetMethod(inlet.methodName);

            // Remove the link.
            LinkUtility.RemoveLinkNodes(
                _instance, outlet.boundEvent,
                targetNode._instance, targetMethod
            );

            // Clear the cache and update information.
            _cachedLinks = null;
            _serializedObject.Update();
        }

        // Remove all links to a given node.
        public void RemoveLinksTo(Node targetNode, Patch patch)
        {
            if (_cachedLinks == null) CacheLinks(patch);

            foreach (var link in _cachedLinks)
                if (link.toNode == targetNode)
                    RemoveLink(link.fromOutlet, link.toNode, link.toInlet);
        }

        // Draw (sub)window GUI.
        public void DrawWindowGUI()
        {
            // Make a rect at the window position. The size is not in use.
            var rect = new Rect(windowPosition, Vector2.one);

            // Show the window.
            var style = isFocused ? GUIStyles.activeNode : GUIStyles.node;
            var newRect = GUILayout.Window(_windowID, rect, OnWindowGUI, displayName, style);

            // Update the serialized info if the position was changed.
            if (newRect.position != rect.position) {
                _serializedObject.Update();
                _serializedPosition.vector2Value = newRect.position;
                _serializedObject.ApplyModifiedProperties();
            }
        }

        // Draw the name field GUI.
        public void DrawNameFieldGUI()
        {
            var newName = EditorGUILayout.DelayedTextField("Node Name", _instance.name);
            if (newName != _instance.name)
            {
                Undo.RecordObject(_instance.gameObject, "Changed Name");
                _instance.name = newName;
            }
        }

        // Window GUI function
        void OnWindowGUI(int id)
        {
            // It can update the button position info on a repaint event.
            var rectUpdate = (Event.current.type == EventType.Repaint);

            // Draw the inlet labels and buttons.
            foreach (var inlet in _inlets)
                if (inlet.DrawGUI(rectUpdate))
                    // The inlet button was pressed; nofity via FeedbackQueue.
                    FeedbackQueue.Enqueue(new FeedbackQueue.InletButtonRecord(this, inlet));

            // Draw the outlet labels and buttons.
            foreach (var outlet in _outlets)
                if (outlet.DrawGUI(rectUpdate))
                    // The outlet button was pressed; nofity via FeedbackQueue.
                    FeedbackQueue.Enqueue(new FeedbackQueue.OutletButtonRecord(this, outlet));

            // The standard GUI behavior.
            _controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            GUI.DragWindow();

            // Is this window clicked?
            if (Event.current.type == EventType.Used)
            {
                // Then assume it's active one.
                _activeWindowID = id;
                // Grab the keyboard focus.
                EditorGUIUtility.keyboardControl = _controlID;
            }

            var e = Event.current;

            if (e.GetTypeForControl(_controlID) == EventType.ValidateCommand)
                if (e.commandName == "Delete" || e.commandName == "SoftDelete")
                    e.Use();

            if (e.GetTypeForControl(_controlID) == EventType.ExecuteCommand)
                if (e.commandName == "Delete" || e.commandName == "SoftDelete")
                    FeedbackQueue.Enqueue(new FeedbackQueue.DeleteNodeRecord(this));
        }
    }
    */
}
