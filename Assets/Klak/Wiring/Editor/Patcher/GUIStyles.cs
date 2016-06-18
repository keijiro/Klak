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
    // GUI styles used in the editor window
    public static class GUIStyles
    {
        #region Public properties

        static public GUIStyle background {
            get {
                if (!_initialized) Initialize();
                return _background;
            }
        }

        static public GUIStyle node {
            get {
                if (!_initialized) Initialize();
                return _node;
            }
        }

        static public GUIStyle activeNode {
            get {
                if (!_initialized) Initialize();
                return _activeNode;
            }
        }

        static public GUIStyle labelLeft {
            get {
                if (!_initialized) Initialize();
                return _labelLeft;
            }
        }

        static public GUIStyle labelRight {
            get {
                if (!_initialized) Initialize();
                return _labelRight;
            }
        }

        static public GUIStyle button {
            get {
                if (!_initialized) Initialize();
                return _button;
            }
        }

        static public GUIStyle horizontalScrollbar {
            get {
                if (!_initialized) Initialize();
                return _horizontalScrollbar;
            }
        }

        static public GUIStyle verticalScrollbar {
            get {
                if (!_initialized) Initialize();
                return _verticalScrollbar;
            }
        }

        #endregion

        #region Private members

        static bool _initialized;

        static GUIStyle _background;
        static GUIStyle _node;
        static GUIStyle _activeNode;
        static GUIStyle _labelLeft;
        static GUIStyle _labelRight;
        static GUIStyle _button;
        static GUIStyle _horizontalScrollbar;
        static GUIStyle _verticalScrollbar;

        public static void Initialize()
        {
            _background = new GUIStyle("flow background");
            _node = new GUIStyle("flow node 0");
            _activeNode = new GUIStyle("flow node 0 on");

            _labelLeft = new GUIStyle("Label");
            _labelRight = new GUIStyle("RightLabel");
            _button = new GUIStyle("miniButton");

            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            _horizontalScrollbar = skin.horizontalScrollbar;
            _verticalScrollbar = skin.verticalScrollbar;

            _initialized = true;
        }

        #endregion
    }
}
