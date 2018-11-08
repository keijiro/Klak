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
using Graphs = UnityEditor.Graphs;

namespace Klak.Wiring.Patcher
{
    // Custom editor styles
    public static class Styles
    {
        static GUIStyle _pinIn;

        public static GUIStyle pinIn
        {
            get {
                if (_pinIn == null) {
                    _pinIn = new GUIStyle(Graphs.Styles.triggerPinIn);
                    _pinIn.stretchWidth = false;
                }
                return _pinIn;
            }
        }

        static GUIStyle _pinOut;

        public static GUIStyle pinOut
        {
            get {
                if (_pinOut == null) {
                    _pinOut = new GUIStyle(Graphs.Styles.triggerPinOut);
                    _pinOut.stretchWidth = false;
                }
                return _pinOut;
            }
        }
    }
}
