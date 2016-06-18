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
using System.Reflection;
using System;

namespace Klak.Wiring.Patcher
{
    public static class EditorUtility
    {
        // Draw bezier line between two nodes.
        public static void DrawCurve(Vector2 p1, Vector2 p2)
        {
            var l = Mathf.Min(Mathf.Abs(p1.y - p2.y), 150);
            var p3 = p1 + new Vector2(l, 0);
            var p4 = p2 - new Vector2(l, 0);
            var c = new Color(0.9f, 0.9f, 0.9f);
            Handles.DrawBezier(p1, p2, p3, p4, c, null, 3);
        }

        // Clears the property drawer cache to avoid the
        // "SerializedObject of SerializedProperty has been Disposed" error.
        public static void ClearPropertyDrawerCache()
        {
            // Call the internal function ScriptAttributeUtility.ClearGlobalCache
            // in a very very bad way!! FIXME FIXME FIXME!!!
            var t = Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor");
            var m = t.GetMethod("ClearGlobalCache", BindingFlags.NonPublic | BindingFlags.Static);
            m.Invoke(null, null);
        }
    }
}
