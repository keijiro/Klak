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

namespace Klak.Wiring.Patcher
{
    // Editor representation of node
    public class Node
    {
        #region Public properties

        // Display name used in UIs
        public string displayName {
            get {
                if (_instance.name == _typeName) return _typeName;
                return _instance.name + " (" + _typeName + ")";
            }
        }

        // Display name of the node type
        public string typeName {
            get { return _typeName; }
        }

        // Validity check
        public bool isValid {
            get { return _instance != null; }
        }

        // Is this window selected in the editor?
        public bool isActive {
            get { return _activeWindowID == _windowID; }
        }

        // Is this window focused (accepting keyboard input)?
        public bool isFocused {
            get { return EditorGUIUtility.keyboardControl == _controlID; }
        }

        // Window position
        public Vector2 windowPosition {
            get { return _serializedPosition.vector2Value; }
        }

        #endregion

        #region Public methods

        // Constructor
        public Node(Wiring.NodeBase instance)
        {
            _instance = instance;
            _typeName = ObjectNames.NicifyVariableName(_instance.GetType().Name);
            _windowID = _windowCounter++;

            // Inlets and outlets
            _inlets = new List<Inlet>();
            _outlets = new List<Outlet>();
            InitializeInletsAndOutlets();

            // Window position
            _serializedObject = new UnityEditor.SerializedObject(_instance);
            _serializedPosition = _serializedObject.FindProperty("_wiringNodePosition");
            ValidatePosition();
        }

        // Check if this is a representation of a given instance.
        public bool IsRepresentationOf(Wiring.NodeBase instance)
        {
            return _instance == instance;
        }

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

        // Create a property editor.
        public Editor CreateEditor()
        {
            return UnityEditor.Editor.CreateEditor(_instance);
        }

        // Draw lines of the links from this node.
        public bool DrawLinkLines(Patch patch)
        {
            // Check if the position information is ready.
            if (_inlets.Count > 0 &&
                _inlets[0].buttonRect.center == Vector2.zero) return false;

            if (_outlets.Count > 0 &&
                _outlets[0].buttonRect.center == Vector2.zero) return false;

            // Make cache and draw all the lines.
            if (_cachedLinks == null) CacheLinks(patch);
            foreach (var link in _cachedLinks) link.DrawLine();
            return true;
        }

        #endregion

        #region Private fields

        // Runtime instance
        Wiring.NodeBase _instance;
        string _typeName;

        // Inlet/outlet list
        List<Inlet> _inlets;
        List<Outlet> _outlets;

        // Cached connection info
        List<NodeLink> _cachedLinks;

        // Serialized property accessor
        SerializedObject _serializedObject;
        SerializedProperty _serializedPosition;

        // GUI
        int _windowID;
        int _controlID;

        // Window ID of currently selected window
        static int _activeWindowID;

        // The total count of windows (used to generate window IDs)
        static int _windowCounter;

        #endregion

        #region Private properties and methods

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

        // Validate the window position.
        void ValidatePosition()
        {
            // Initialize the node position if not yet.
            var position = _serializedPosition.vector2Value;
            if (position == Wiring.NodeBase.uninitializedNodePosition)
            {
                // Calculate the initial window position with the window ID.
                var x = (_windowID % 8 + 1) * 50;
                var y = (_windowID % 16 + 1) * 20;
                _serializedPosition.vector2Value = new Vector2(x, y);
                _serializedObject.ApplyModifiedProperties();
            }
        }

        // Initialize all inlets/outlets from the node instance with using reflection.
        void InitializeInletsAndOutlets()
        {
            // Enumeration flags: all public and non-public members
            const BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            // Inlets (property)
            foreach (var prop in _instance.GetType().GetProperties(flags))
            {
                var attrs = prop.GetCustomAttributes(typeof(Wiring.InletAttribute), true);
                if (attrs.Length == 0) continue;

                _inlets.Add(new Inlet(prop.GetSetMethod().Name, prop.Name));
            }

            // Inlets (method)
            foreach (var method in _instance.GetType().GetMethods(flags))
            {
                var attrs = method.GetCustomAttributes(typeof(Wiring.InletAttribute), true);
                if (attrs.Length == 0) continue;
                
                _inlets.Add(new Inlet(method.Name, method.Name));
            }

            // Outlets (UnityEvent members)
            foreach (var field in _instance.GetType().GetFields(flags))
            {
                var attrs = field.GetCustomAttributes(typeof(Wiring.OutletAttribute), true);
                if (attrs.Length == 0) continue;

                var evt = (UnityEventBase)field.GetValue(_instance);
                _outlets.Add(new Outlet(field.Name, evt));
            }
        }

        // Get an inlet with a given name.
        Inlet GetInletWithName(string name)
        {
            foreach (var inlet in _inlets)
                if (inlet.methodName == name) return inlet;
            return null;
        }

        // Enumerate all links from the outlets and cache them.
        void CacheLinks(Patch patch)
        {
            _cachedLinks = new List<NodeLink>();

            foreach (var outlet in _outlets)
            {
                // Scan all the events from the outlet.
                var boundEvent = outlet.boundEvent;
                var targetCount = boundEvent.GetPersistentEventCount();
                for (var i = 0; i < targetCount; i++)
                {
                    var target = boundEvent.GetPersistentTarget(i);

                    // Ignore it if it's a null event or the target is not a node.
                    if (target == null || !(target is Wiring.NodeBase)) continue;

                    // Try to retrieve the linked inlet.
                    var targetNode = patch.GetNodeOfInstance((Wiring.NodeBase)target);
                    var methodName = boundEvent.GetPersistentMethodName(i);
                    var inlet = targetNode.GetInletWithName(methodName);

                    // Cache it if it's a valid link.
                    if (targetNode != null && inlet != null)
                        _cachedLinks.Add(new NodeLink(this, outlet, targetNode, inlet));
                }
            }
        }

        #endregion
    }
}
