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
using System.Reflection;
using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    // Specialized edge drawer class
    public class EdgeGUI : Graphs.IEdgeGUI
    {
        #region Public members

        public Graphs.GraphGUI host { get; set; }
        public List<int> edgeSelection { get; set; }

        public EdgeGUI()
        {
            edgeSelection = new List<int>();
        }

        #endregion

        #region IEdgeGUI implementation

        public void DoEdges()
        {
            // Draw edges on repaint.
            if (Event.current.type == EventType.Repaint)
            {
                var colorOff = Color.white;
                var colorOn = new Color(0.6f, 0.75f, 1);

                var i = 0;
                foreach (var edge in host.graph.edges)
                {
                    if (edge == _moveEdge) continue;
                    DrawEdge(edge, edgeSelection.Contains(i) ? colorOn : colorOff);
                    i++;
                }
            }

            // Terminate dragging operation on mouse button up.
            if (Event.current.type == EventType.MouseUp && _dragSourceSlot != null)
            {
                if (_moveEdge != null)
                {
                    host.graph.RemoveEdge(_moveEdge);
                    _moveEdge = null;
                }

                if (_dropTarget == null)
                {
                    EndDragging();
                    Event.current.Use();
                }
            }
        }

        public void DoDraggedEdge()
        {
            if (_dragSourceSlot == null) return;

            var eventType = Event.current.GetTypeForControl(0);

            if (eventType == EventType.Repaint)
            {
                // Draw the working edge.
                var p1 = GetPositionAsFromSlot(_dragSourceSlot);
                var p2 = _dropTarget != null ?
                    GetPositionAsToSlot(_dropTarget) :
                    Event.current.mousePosition;
                DrawEdge(p1, p2, Color.white);
            }

            if (eventType == EventType.MouseDrag)
            {
                // Discard the last candidate.
                _dropTarget = null;
                Event.current.Use();
            }
        }

        public void BeginSlotDragging(Graphs.Slot slot, bool allowStartDrag, bool allowEndDrag)
        {
            if (allowStartDrag)
            {
                // Start dragging with a new connection.
                _dragSourceSlot = slot;
                Event.current.Use();
            }

            if (allowEndDrag && slot.edges.Count > 0)
            {
                // Start dragging to modify an existing connection.
                _moveEdge = slot.edges[slot.edges.Count - 1];
                _dragSourceSlot = _moveEdge.fromSlot;
                _dropTarget = slot;
                Event.current.Use();
            }
        }

        public void SlotDragging(Graphs.Slot slot, bool allowEndDrag, bool allowMultiple)
        {
            Debug.Assert(allowMultiple);

            if (!allowEndDrag) return;
            if (_dragSourceSlot == null || _dragSourceSlot == slot) return;

            // Is this slot can be a drop target?
            if (_dropTarget != slot &&
                slot.node.graph.CanConnect(_dragSourceSlot, slot) &&
                !slot.node.graph.Connected(_dragSourceSlot, slot))
            {
                _dropTarget = slot;
            }

            Event.current.Use();
        }

        public void EndSlotDragging(Graphs.Slot slot, bool allowMultiple)
        {
            Debug.Assert(allowMultiple);

            // Do nothing if no target was specified.
            if (_dropTarget != slot) return;

            // If we're going to modify an existing edge, remove it.
            if (_moveEdge != null) slot.node.graph.RemoveEdge(_moveEdge);

            // Try to connect the edge to the target.
            try
            {
                slot.node.graph.Connect(_dragSourceSlot, slot);
            }
            finally
            {
                EndDragging();
                slot.node.graph.Dirty();
                Event.current.Use();
            }

            UnityEngine.GUIUtility.ExitGUI();
        }

        public void EndDragging()
        {
            _dragSourceSlot = _dropTarget = null;
            _moveEdge = null;
        }

        public Graphs.Edge FindClosestEdge()
        {
            // Target position
            var point = Event.current.mousePosition;

            // Candidate
            var minDist = Mathf.Infinity;
            var found = null as Graphs.Edge;

            // Scan all the edges in the graph.
            foreach (var edge in host.graph.edges)
            {
                var p1 = GetPositionAsFromSlot(edge.fromSlot);
                var p2 = GetPositionAsToSlot(edge.toSlot);
                var dist = HandleUtility.DistancePointLine(point, p1, p2);

                // Record the current edge if the point is the closest one.
                if (dist < minDist)
                {
                    minDist = dist;
                    found = edge;
                }
            }

            // Discard the result if it's too far.
            return minDist < 12.0f ? found : null;
        }

        #endregion

        #region Private members

        Graphs.Edge _moveEdge;
        Graphs.Slot _dragSourceSlot;
        Graphs.Slot _dropTarget;

        #endregion

        #region Edge drawer

        const float kEdgeWidth = 8;
        const float kEdgeSlotYOffset = 9;

        static void DrawEdge(Graphs.Edge edge, Color color)
        {
            var p1 = GetPositionAsFromSlot(edge.fromSlot);
            var p2 = GetPositionAsToSlot(edge.toSlot);
            DrawEdge(p1, p2, color * edge.color);
        }

        static void DrawEdge(Vector2 p1, Vector2 p2, Color color)
        {
            var l = Mathf.Min(Mathf.Abs(p1.y - p2.y), 50);
            var p3 = p1 + new Vector2(l, 0);
            var p4 = p2 - new Vector2(l, 0);
            var texture = (Texture2D)Graphs.Styles.connectionTexture.image;
            Handles.DrawBezier(p1, p2, p3, p4, color, texture, kEdgeWidth);
        }

        #endregion

        #region Utilities to access private members

        // Accessors for Slot.m_Position
        static Rect GetSlotPosition(Graphs.Slot slot)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var func = typeof(Graphs.Slot).GetField("m_Position", flags);
            return (Rect)func.GetValue(slot);
        }

        static Vector2 GetPositionAsFromSlot(Graphs.Slot slot)
        {
            var rect = GetSlotPosition(slot);
            return GUIClip(new Vector2(rect.xMax, rect.y + kEdgeSlotYOffset));
        }

        static Vector2 GetPositionAsToSlot(Graphs.Slot slot)
        {
            var rect = GetSlotPosition(slot);
            return GUIClip(new Vector2(rect.x, rect.y + kEdgeSlotYOffset));
        }

        // Caller for GUIClip.Clip
        static Vector2 GUIClip(Vector2 pos)
        {
            var type = Type.GetType("UnityEngine.GUIClip,UnityEngine");
            var method = type.GetMethod("Clip", new Type[]{ typeof(Vector2) });
            return (Vector2)method.Invoke(null, new object[]{ pos });
        }

        #endregion
    }
}
