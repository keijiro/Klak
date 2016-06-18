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

namespace Klak.Wiring.Patcher
{
    // Link between a pair of nodes
    // Mainly used for caching link infromation.
    public class NodeLink
    {
        #region Public properties and methods

        // Accessors
        public Node fromNode     { get { return _fromNode;   } }
        public Node toNode       { get { return _toNode;     } }
        public Outlet fromOutlet { get { return _fromOutlet; } }
        public Inlet toInlet     { get { return _toInlet;    } }

        // Constructor
        public NodeLink(
            Node fromNode, Outlet fromOutlet,
            Node toNode, Inlet toInlet
        )
        {
            _fromNode = fromNode;
            _fromOutlet = fromOutlet;
            _toNode = toNode;
            _toInlet = toInlet;
        }

        // Draw a line (curve) between the nodes.
        public void DrawLine()
        {
            var p1 = (Vector3)_fromNode.windowPosition;
            var p2 = (Vector3)_toNode.windowPosition;

            p1 += (Vector3)_fromOutlet.buttonRect.center;
            p2 += (Vector3)_toInlet.buttonRect.center;

            EditorUtility.DrawCurve(p1, p2);
        }

        #endregion

        #region Private fields

        Node _fromNode;
        Outlet _fromOutlet;

        Node _toNode;
        Inlet _toInlet;

        #endregion
    }
}
